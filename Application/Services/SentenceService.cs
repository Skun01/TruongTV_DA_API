using Application.Common;
using Application.Helper;
using Application.Mappings;
using Application.DTOs.Sentences;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class SentenceService : ISentenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoicevoxService _voicevoxService;

    public SentenceService(IUnitOfWork unitOfWork, IVoicevoxService voicevoxService)
    {
        _unitOfWork = unitOfWork;
        _voicevoxService = voicevoxService;
    }

    public Task<ImportSentenceRequest> GetImportTemplateAsync()
    {
        var template = new ImportSentenceRequest
        {
            Items = new List<ImportSentenceItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Text = "日本へ行きたいです。",
                    Meaning = "Tôi muốn đi Nhật.",
                    SpeakerId = 3,
                    Level = "N5",
                },
            },
        };

        return Task.FromResult(template);
    }

    public async Task<ImportSentenceRequest> ExportAsync(SentenceExportQuery query, string currentUserId)
    {
        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, _) = await _unitOfWork.Sentences.SearchAsync(
            query.Q,
            levelEnum,
            createdBy,
            query.HasAudio,
            1,
            int.MaxValue);

        return new ImportSentenceRequest
        {
            Items = items.Select(item => item.ToImportItem()).ToList(),
        };
    }

    public async Task<SentenceImportPreviewResponse> PreviewImportAsync(ImportSentenceRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new AppException(MessageConstants.SentenceMessage.IMPORT_INVALID_PAYLOAD, 400);

        var previewItems = new List<SentenceImportPreviewItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var previewItem = new SentenceImportPreviewItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Text = item.Text?.Trim() ?? string.Empty,
            };

            ValidateImportItem(item, previewItem);
            previewItem.IsValid = previewItem.Errors.Count == 0;
            previewItems.Add(previewItem);
        }

        return new SentenceImportPreviewResponse
        {
            TotalItems = previewItems.Count,
            ValidItems = previewItems.Count(item => item.IsValid),
            InvalidItems = previewItems.Count(item => !item.IsValid),
            Items = previewItems,
        };
    }

    public async Task<SentenceImportCommitResponse> CommitImportAsync(ImportSentenceRequest request, string currentUserId)
    {
        var preview = await PreviewImportAsync(request);
        if (preview.InvalidItems > 0)
            return BuildBlockedCommitResponse(preview);

        var commitItems = new List<SentenceImportCommitItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var commitItem = new SentenceImportCommitItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Text = item.Text?.Trim() ?? string.Empty,
            };

            try
            {
                var result = await CreateAsync(item.ToCreateRequest(), currentUserId);
                commitItem.IsSuccess = true;
                commitItem.Action = "created";
                commitItem.SentenceId = result.Id;
            }
            catch (AppException ex)
            {
                commitItem.Errors.Add(ex.ErrorCode);
            }
            catch (ApplicationException ex)
            {
                commitItem.Errors.Add(ex.Message);
            }
            catch
            {
                commitItem.Errors.Add(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);
            }

            commitItems.Add(commitItem);
        }

        return new SentenceImportCommitResponse
        {
            TotalItems = commitItems.Count,
            SuccessfulItems = commitItems.Count(item => item.IsSuccess),
            FailedItems = commitItems.Count(item => !item.IsSuccess),
            HasValidationErrors = false,
            Items = commitItems,
        };
    }

    public async Task<SentenceResponse> CreateAsync(CreateSentenceRequest request, string currentUserId)
    {
        var text = request.Text.Trim();
        var synthesisResult = await _voicevoxService.SynthesizeAsync(text, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        var sentence = new Sentence
        {
            Id = Guid.NewGuid().ToString(),
            Text = text,
            Meaning = request.Meaning.Trim(),
            AudioUrl = synthesisResult.AudioUrl,
            SpeakerId = synthesisResult.SpeakerId,
            Level = EnumParsingHelper.ParseNullable<Domain.Enums.JlptLevel>(request.Level),
            CreatedBy = currentUserId,
        };

        await _unitOfWork.Sentences.AddAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return sentence.ToResponse();
    }

    public async Task<SentenceResponse> GetByIdAsync(string id)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        return sentence.ToResponse();
    }

    public async Task<(List<SentenceResponse> Items, MetaData Meta)> SearchAsync(SentenceSearchQuery query, string currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var levelEnum = EnumParsingHelper.ParseNullable<Domain.Enums.JlptLevel>(query.Level);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Sentences.SearchAsync(
            query.Q,
            levelEnum,
            createdBy,
            query.HasAudio,
            page,
            pageSize);

        var mappedItems = items.Select(item => item.ToResponse()).ToList();
        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mappedItems, meta);
    }

    public async Task<SentenceResponse> UpdateAsync(string id, UpdateSentenceRequest request)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var text = request.Text.Trim();
        var synthesisResult = await _voicevoxService.SynthesizeAsync(text, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        sentence.Text = text;
        sentence.Meaning = request.Meaning.Trim();
        sentence.AudioUrl = synthesisResult.AudioUrl;
        sentence.SpeakerId = synthesisResult.SpeakerId;
        sentence.Level = EnumParsingHelper.ParseNullable<Domain.Enums.JlptLevel>(request.Level);
        sentence.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sentences.UpdateAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return sentence.ToResponse();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var sentence = await _unitOfWork.Sentences.GetByIdAsync(id);
        if (sentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        _unitOfWork.Sentences.DeleteAsync(sentence);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static SentenceImportCommitResponse BuildBlockedCommitResponse(SentenceImportPreviewResponse preview)
    {
        var items = preview.Items.Select(item => new SentenceImportCommitItemResponse
        {
            RowNumber = item.RowNumber,
            Text = item.Text,
            IsSuccess = false,
            Action = "skipped",
            Errors = item.IsValid
                ? new List<string> { MessageConstants.SentenceMessage.IMPORT_BATCH_HAS_ERRORS }
                : item.Errors.ToList(),
        }).ToList();

        return new SentenceImportCommitResponse
        {
            TotalItems = preview.TotalItems,
            SuccessfulItems = 0,
            FailedItems = items.Count,
            HasValidationErrors = true,
            Items = items,
        };
    }

    private static void ValidateImportItem(
        ImportSentenceItemRequest item,
        SentenceImportPreviewItemResponse previewItem)
    {
        if (previewItem.RowNumber <= 0)
            previewItem.Errors.Add("rowNumber must be greater than 0.");

        ValidateRequiredText(item.Text, "text", 500, previewItem.Errors);
        ValidateRequiredText(item.Meaning, "meaning", 500, previewItem.Errors);
        ValidateOptionalText(item.Level, "level", 10, previewItem.Errors);
        ValidateOptionalEnum<JlptLevel>(item.Level, "level", previewItem.Errors);

        if (item.SpeakerId.HasValue && item.SpeakerId.Value <= 0)
            previewItem.Errors.Add("speakerId must be greater than 0.");
    }

    private static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add($"{fieldName} must not exceed {maxLength} characters.");
    }

    private static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add($"{fieldName} must not exceed {maxLength} characters.");
    }

    private static void ValidateOptionalEnum<TEnum>(
        string? value,
        string fieldName,
        List<string> errors) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add($"{fieldName} is invalid.");
    }
}
