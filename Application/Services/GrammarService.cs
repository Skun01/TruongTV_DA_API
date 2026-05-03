using Application.Common;
using Application.DTOs.Grammar;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class GrammarService : IGrammarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITextToSpeechService _ttsService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<GrammarService> _logger;

    public GrammarService(
        IUnitOfWork unitOfWork,
        ITextToSpeechService ttsService,
        IFileUploadService fileUploadService,
        ILogger<GrammarService> logger)
    {
        _unitOfWork = unitOfWork;
        _ttsService = ttsService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<GrammarDetailResponse> GetDetailAsync(string cardId, string? currentUserId)
    {
        var card = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(cardId);
        if (card == null || card.GrammarDetail == null || card.CardType != CardType.Grammar)
            throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        EnsureCardReadable(card, currentUserId);

        var relations = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(cardId);
        var resources = await _unitOfWork.GrammarResources.GetByCardIdAsync(cardId);
        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);

        return card.ToGrammarDetailResponse(relations, resources, notes, currentUserId);
    }

    public async Task<(List<GrammarListItemResponse> Items, MetaData Meta)> SearchAsync(GrammarSearchQuery query, string currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var registerEnum = EnumParsingHelper.ParseNullable<RegisterType>(query.Register);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchGrammarAsync(
            query.Q,
            levelEnum,
            statusEnum,
            registerEnum,
            createdBy,
            page,
            pageSize);

        var mapped = items.Select(item => item.ToGrammarListItemResponse()).ToList();
        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mapped, meta);
    }

    public Task<ImportGrammarRequest> GetImportTemplateAsync()
    {
        return Task.FromResult(GrammarImportHelper.CreateTemplate());
    }

    public async Task<ImportGrammarRequest> ExportAsync(GrammarExportQuery query, string currentUserId)
    {
        var searchQuery = new GrammarSearchQuery
        {
            Q = query.Q,
            Level = query.Level,
            Status = query.Status,
            Register = query.Register,
            CreatedByMe = query.CreatedByMe,
            Page = 1,
            PageSize = int.MaxValue,
        };

        var (items, _) = await SearchAsync(searchQuery, currentUserId);
        var resultItems = new List<ImportGrammarItemRequest>();

        foreach (var item in items)
        {
            var card = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(item.Id);
            if (card == null || card.GrammarDetail == null || card.CardType != CardType.Grammar)
                continue;

            var relations = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(item.Id);
            var resources = await _unitOfWork.GrammarResources.GetByCardIdAsync(item.Id);
            resultItems.Add(card.ToImportItem(relations, resources));
        }

        return new ImportGrammarRequest
        {
            Items = resultItems,
        };
    }

    public async Task<GrammarImportPreviewResponse> PreviewImportAsync(ImportGrammarRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new AppException(MessageConstants.GrammarMessage.IMPORT_INVALID_PAYLOAD, 400);

        var previewItems = new List<GrammarImportPreviewItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var previewItem = new GrammarImportPreviewItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Title = item.Title?.Trim() ?? string.Empty,
            };

            await GrammarImportHelper.ValidateImportItemAsync(_unitOfWork, item, previewItem);
            previewItem.IsValid = previewItem.Errors.Count == 0;
            previewItems.Add(previewItem);
        }

        return new GrammarImportPreviewResponse
        {
            TotalItems = previewItems.Count,
            ValidItems = previewItems.Count(item => item.IsValid),
            InvalidItems = previewItems.Count(item => !item.IsValid),
            Items = previewItems,
        };
    }

    public async Task<GrammarImportCommitResponse> CommitImportAsync(ImportGrammarRequest request, string currentUserId)
    {
        var preview = await PreviewImportAsync(request);
        if (preview.InvalidItems > 0)
            return GrammarImportHelper.BuildBlockedCommitResponse(preview);

        var commitItems = new List<GrammarImportCommitItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var commitItem = new GrammarImportCommitItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Title = item.Title?.Trim() ?? string.Empty,
            };

            try
            {
                var result = await CreateAsync(item.ToCreateRequest(), currentUserId);
                commitItem.IsSuccess = true;
                commitItem.Action = "created";
                commitItem.CardId = result.Id;
            }
            catch (AppException ex)
            {
                commitItem.Errors.Add(ex.ErrorCode);
            }
            catch (ApplicationException ex)
            {
                commitItem.Errors.Add(ex.Message);
                _logger.LogError(
                    ex,
                    "Grammar import failed with application error at row {RowNumber}. Title: {Title}",
                    commitItem.RowNumber,
                    commitItem.Title);
            }
            catch (Exception ex)
            {
                commitItem.Errors.Add(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);
                _logger.LogError(
                    ex,
                    "Grammar import failed with unexpected error at row {RowNumber}. Title: {Title}",
                    commitItem.RowNumber,
                    commitItem.Title);
            }

            commitItems.Add(commitItem);
        }

        return new GrammarImportCommitResponse
        {
            TotalItems = commitItems.Count,
            SuccessfulItems = commitItems.Count(item => item.IsSuccess),
            FailedItems = commitItems.Count(item => !item.IsSuccess),
            HasValidationErrors = false,
            Items = commitItems,
        };
    }

    public async Task<GrammarDetailResponse> CreateAsync(CreateGrammarCardRequest request, string currentUserId)
    {
        var cardId = Guid.NewGuid().ToString();
        var card = new Card
        {
            Id = cardId,
            CardType = CardType.Grammar,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
            Tags = StringHelper.NormalizeList(request.Tags),
            Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        card.GrammarDetail = new GrammarDetail
        {
            CardId = cardId,
            Structures = NormalizeStructures(request.Structures),
            Explanation = GrammarMarkdownHelper.NormalizeOptional(request.Explanation, "explanation", 10000),
            Caution = GrammarMarkdownHelper.NormalizeOptional(request.Caution, "caution", 5000),
            Register = EnumParsingHelper.ParseNullable<RegisterType>(request.Register),
            AlternateForms = StringHelper.NormalizeList(request.AlternateForms),
        };

        await ValidateAndSyncRelationsAsync(cardId, request.Relations);
        await SyncResourcesAsync(cardId, request.Resources);

        await _unitOfWork.Cards.AddAsync(card);
        await SyncGrammarSentencesAsync(cardId, request.Sentences, currentUserId);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        return created.ToGrammarDetailResponse(
            await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(cardId),
            await _unitOfWork.GrammarResources.GetByCardIdAsync(cardId),
            new List<UserCardNote>(),
            currentUserId);
    }

    public async Task<GrammarDetailResponse> UpdateAsync(string cardId, UpdateGrammarCardRequest request, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Grammar)
            throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        var detail = await _unitOfWork.GrammarDetails.GetByIdAsync(cardId);
        if (detail == null)
            throw new AppException(MessageConstants.GrammarMessage.DETAIL_NOT_FOUND, 404);

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Structures = NormalizeStructures(request.Structures);
        detail.Explanation = GrammarMarkdownHelper.NormalizeOptional(request.Explanation, "explanation", 10000);
        detail.Caution = GrammarMarkdownHelper.NormalizeOptional(request.Caution, "caution", 5000);
        detail.Register = EnumParsingHelper.ParseNullable<RegisterType>(request.Register);
        detail.AlternateForms = StringHelper.NormalizeList(request.AlternateForms);

        await ValidateAndSyncRelationsAsync(cardId, request.Relations);
        await SyncResourcesAsync(cardId, request.Resources);
        await SyncGrammarSentencesAsync(cardId, request.Sentences, currentUserId);

        _unitOfWork.Cards.UpdateAsync(card);
        _unitOfWork.GrammarDetails.UpdateAsync(detail);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Cards.GetGrammarDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        var relations = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(cardId);
        var resources = await _unitOfWork.GrammarResources.GetByCardIdAsync(cardId);
        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return updated.ToGrammarDetailResponse(relations, resources, notes, currentUserId);
    }

    public async Task<bool> SoftDeleteAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Grammar)
            throw new AppException(MessageConstants.GrammarMessage.CARD_NOT_FOUND, 404);

        card.Status = PublishStatus.Archived;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new AppException(MessageConstants.GrammarMessage.READ_FORBIDDEN, 401);
    }

    private static List<Domain.ValueObjects.GrammarStructureItem> NormalizeStructures(List<GrammarStructureRequest> structures)
    {
        var result = new List<Domain.ValueObjects.GrammarStructureItem>();

        for (var i = 0; i < structures.Count; i++)
        {
            var structure = structures[i];
            var normalizedPattern = GrammarMarkdownHelper.NormalizeRequired(
                structure.Pattern,
                $"structures[{i}].pattern",
                1000);

            Dictionary<string, string>? normalizedAnnotations = null;
            if (structure.Annotations != null && structure.Annotations.Count > 0)
            {
                normalizedAnnotations = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var annotation in structure.Annotations)
                {
                    var key = annotation.Key.Trim();
                    if (string.IsNullOrWhiteSpace(key))
                        throw new AppException(
                            MessageConstants.GrammarMessage.INVALID_RICH_TEXT,
                            400,
                            details: new { field = $"structures[{i}].annotations", reason = "Annotation key is required." });

                    var normalizedValue = GrammarMarkdownHelper.NormalizeRequired(
                        annotation.Value,
                        $"structures[{i}].annotations.{key}",
                        1000);

                    normalizedAnnotations[key] = normalizedValue;
                }
            }

            result.Add(new Domain.ValueObjects.GrammarStructureItem
            {
                Pattern = normalizedPattern,
                Annotations = normalizedAnnotations,
            });
        }

        return result;
    }

    private async Task ValidateAndSyncRelationsAsync(string grammarId, List<GrammarRelationUpsertRequest> relations)
    {
        var existing = await _unitOfWork.GrammarRelations.GetByGrammarIdAsync(grammarId);
        foreach (var relation in existing)
        {
            _unitOfWork.GrammarRelations.DeleteAsync(relation);
        }

        var uniqueSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relation in relations)
        {
            var relatedId = relation.RelatedId.Trim();
            if (string.IsNullOrWhiteSpace(relatedId) || relatedId == grammarId)
                throw new AppException(MessageConstants.GrammarMessage.INVALID_RELATION, 400);

            var relationType = EnumParsingHelper.ParseNullable<GrammarRelationType>(relation.RelationType);
            if (!relationType.HasValue)
                throw new AppException(MessageConstants.GrammarMessage.INVALID_RELATION, 400);

            var relatedCard = await _unitOfWork.Cards.GetByIdAsync(relatedId);
            if (relatedCard == null || relatedCard.CardType != CardType.Grammar)
                throw new AppException(MessageConstants.GrammarMessage.RELATED_CARD_NOT_FOUND, 404);

            var dedupKey = $"{relatedId}:{relationType.Value}";
            if (!uniqueSet.Add(dedupKey))
                continue;

            await _unitOfWork.GrammarRelations.AddAsync(new GrammarRelation
            {
                GrammarId = grammarId,
                RelatedId = relatedId,
                RelationType = relationType.Value,
            });
        }
    }

    private async Task SyncResourcesAsync(string grammarId, List<GrammarResourceUpsertRequest> resources)
    {
        var existing = await _unitOfWork.GrammarResources.GetByCardIdAsync(grammarId);
        foreach (var resource in existing)
        {
            _unitOfWork.GrammarResources.DeleteAsync(resource);
        }

        foreach (var resource in resources)
        {
            await _unitOfWork.GrammarResources.AddAsync(new GrammarResource
            {
                Id = Guid.NewGuid().ToString(),
                CardId = grammarId,
                Title = resource.Title.Trim(),
                Url = resource.Url.Trim(),
            });
        }
    }

    private async Task SyncGrammarSentencesAsync(
        string cardId,
        List<GrammarSentenceUpsertRequest> requests,
        string currentUserId)
    {
        var existingLinks = await _unitOfWork.CardSentences.GetByCardIdAsync(cardId);
        var keptSentenceIds = new HashSet<string>();

        foreach (var request in requests)
        {
            var sentence = await UpsertGrammarSentenceAsync(request, currentUserId);
            if (!keptSentenceIds.Add(sentence.Id))
                continue;

            var existingLink = existingLinks.FirstOrDefault(link => link.SentenceId == sentence.Id);
            if (existingLink != null)
            {
                existingLink.Position = request.Position;
                existingLink.BlankWord = StringHelper.NormalizeOptional(request.BlankWord);
                existingLink.Hint = StringHelper.NormalizeOptional(request.Hint);
                existingLink.AnswerList = StringHelper.NormalizeAnswerList(request.AnswerList, request.BlankWord);
                _unitOfWork.CardSentences.UpdateAsync(existingLink);
                continue;
            }

            await _unitOfWork.CardSentences.AddAsync(new CardSentence
            {
                CardId = cardId,
                SentenceId = sentence.Id,
                Position = request.Position,
                BlankWord = StringHelper.NormalizeOptional(request.BlankWord),
                Hint = StringHelper.NormalizeOptional(request.Hint),
                AnswerList = StringHelper.NormalizeAnswerList(request.AnswerList, request.BlankWord),
            });
        }

        foreach (var link in existingLinks.Where(link => !keptSentenceIds.Contains(link.SentenceId)))
        {
            _unitOfWork.CardSentences.DeleteAsync(link);
        }
    }

    private async Task<Sentence> UpsertGrammarSentenceAsync(
        GrammarSentenceUpsertRequest request,
        string currentUserId)
    {
        var text = request.Text.Trim();
        var audioResult = await AzureTtsHelper.SynthesizeAndUploadAsync(
            _ttsService,
            _fileUploadService,
            text,
            currentUserId,
            $"sent_{Guid.NewGuid():N}.mp3",
            MessageConstants.SentenceMessage.AUDIO_SYNTHESIS_FAILED,
            _logger);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            var sentence = new Sentence
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Meaning = request.Meaning.Trim(),
                AudioUrl = audioResult.AudioUrl,
                Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
                CreatedBy = currentUserId,
            };

            await _unitOfWork.Sentences.AddAsync(sentence);
            return sentence;
        }

        var existingSentence = await _unitOfWork.Sentences.GetByIdAsync(request.Id);
        if (existingSentence == null)
            throw new AppException(MessageConstants.SentenceMessage.NOT_FOUND, 404);

        existingSentence.Text = text;
        existingSentence.Meaning = request.Meaning.Trim();
        existingSentence.AudioUrl = audioResult.AudioUrl;
        existingSentence.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        existingSentence.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sentences.UpdateAsync(existingSentence);
        return existingSentence;
    }
}
