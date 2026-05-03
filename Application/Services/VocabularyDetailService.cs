using Application.Common;
using Application.DTOs.CardNotes;
using Application.DTOs.Vocabulary;
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

public class VocabularyDetailService : IVocabularyDetailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITextToSpeechService _ttsService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<VocabularyDetailService> _logger;

    public VocabularyDetailService(
        IUnitOfWork unitOfWork,
        ITextToSpeechService ttsService,
        IFileUploadService fileUploadService,
        ILogger<VocabularyDetailService> logger)
    {
        _unitOfWork = unitOfWork;
        _ttsService = ttsService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string? currentUserId)
    {
        var card = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId);
        if (card == null || card.VocabularyDetail == null || card.CardType != CardType.Vocab)
            throw new AppException(MessageConstants.VocabularyMessage.CARD_NOT_FOUND, 404);

        EnsureCardReadable(card, currentUserId);

        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return card.ToDetailResponse(notes, currentUserId);
    }

    public async Task<(List<VocabularyListItemResponse> Items, MetaData Meta)> SearchAsync(VocabularySearchQuery query, string currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var wordTypeEnum = EnumParsingHelper.ParseNullable<WordType>(query.WordType);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var (items, total) = await _unitOfWork.Cards.SearchVocabularyAsync(
            query.Q,
            levelEnum,
            statusEnum,
            wordTypeEnum,
            query.HasAudio,
            createdBy,
            page,
            pageSize);

        var mapped = items.Select(item => item.ToListItemResponse()).ToList();

        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mapped, meta);
    }

    public Task<ImportVocabularyRequest> GetImportTemplateAsync()
    {
        return Task.FromResult(VocabularyImportHelper.CreateTemplate());
    }

    public async Task<ImportVocabularyRequest> ExportAsync(VocabularyExportQuery query, string currentUserId)
    {
        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);
        var statusEnum = EnumParsingHelper.ParseNullable<PublishStatus>(query.Status);
        var wordTypeEnum = EnumParsingHelper.ParseNullable<WordType>(query.WordType);
        var createdBy = query.CreatedByMe ? currentUserId : null;

        var items = await _unitOfWork.Cards.GetVocabularyExportAsync(
            query.Q,
            levelEnum,
            statusEnum,
            wordTypeEnum,
            query.HasAudio,
            createdBy);

        return new ImportVocabularyRequest
        {
            Items = items.Select(item => item.ToImportItem()).ToList(),
        };
    }

    public async Task<VocabularyImportPreviewResponse> PreviewImportAsync(ImportVocabularyRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new AppException(MessageConstants.VocabularyMessage.IMPORT_INVALID_PAYLOAD, 400);

        var previewItems = new List<VocabularyImportPreviewItemResponse>();
        var batchWritingSet = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var previewItem = new VocabularyImportPreviewItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Title = item.Title?.Trim() ?? string.Empty,
                Writing = item.Writing?.Trim() ?? string.Empty,
            };

            await VocabularyImportHelper.ValidateImportItemAsync(_unitOfWork, item, previewItem, batchWritingSet);
            previewItem.IsValid = previewItem.Errors.Count == 0;
            previewItems.Add(previewItem);
        }

        return new VocabularyImportPreviewResponse
        {
            TotalItems = previewItems.Count,
            ValidItems = previewItems.Count(item => item.IsValid),
            InvalidItems = previewItems.Count(item => !item.IsValid),
            Items = previewItems,
        };
    }

    public async Task<VocabularyImportCommitResponse> CommitImportAsync(ImportVocabularyRequest request, string currentUserId)
    {
        var preview = await PreviewImportAsync(request);
        if (preview.InvalidItems > 0)
            return VocabularyImportHelper.BuildBlockedCommitResponse(preview);

        var commitItems = new List<VocabularyImportCommitItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var commitItem = new VocabularyImportCommitItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Title = item.Title?.Trim() ?? string.Empty,
                Writing = item.Writing?.Trim() ?? string.Empty,
            };

            try
            {
                var result = await CreateAsync(item.ToCreateRequest(), currentUserId);
                commitItem.Action = "created";

                commitItem.IsSuccess = true;
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
                    "Vocabulary import failed with application error at row {RowNumber}. Writing: {Writing}",
                    commitItem.RowNumber,
                    commitItem.Writing);
            }
            catch (Exception ex)
            {
                commitItem.Errors.Add(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);
                _logger.LogError(
                    ex,
                    "Vocabulary import failed with unexpected error at row {RowNumber}. Writing: {Writing}",
                    commitItem.RowNumber,
                    commitItem.Writing);
            }

            commitItems.Add(commitItem);
        }

        return new VocabularyImportCommitResponse
        {
            TotalItems = commitItems.Count,
            SuccessfulItems = commitItems.Count(item => item.IsSuccess),
            FailedItems = commitItems.Count(item => !item.IsSuccess),
            HasValidationErrors = false,
            Items = commitItems,
        };
    }

    public async Task<VocabularyDetailResponse> CreateAsync(CreateVocabularyCardRequest request, string currentUserId)
    {
        var cardId = Guid.NewGuid().ToString();
        var writing = request.Writing.Trim();
        var reading = StringHelper.NormalizeOptional(request.Reading);
        var synthesisText = ResolveVocabularySynthesisText(writing, reading);

        var audioResult = await AzureTtsHelper.SynthesizeAndUploadAsync(
            _ttsService,
            _fileUploadService,
            synthesisText,
            currentUserId,
            $"vocab_{cardId}.mp3",
            MessageConstants.VocabularyMessage.AUDIO_SYNTHESIS_FAILED,
            _logger);

        var finalPitchPattern = request.PitchPattern;

        var card = new Card
        {
            Id = cardId,
            CardType = CardType.Vocab,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
            Tags = StringHelper.NormalizeList(request.Tags),
            Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        card.VocabularyDetail = new VocabularyDetail
        {
            CardId = cardId,
            Writing = writing,
            Reading = reading,
            PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern),
            AudioUrl = audioResult.AudioUrl,
            WordType = EnumParsingHelper.ParseNullable<WordType>(request.WordType),
            Meanings = VocabularyHelper.MapMeaningItems(request.Meanings),
            Synonyms = StringHelper.NormalizeList(request.Synonyms),
            Antonyms = StringHelper.NormalizeList(request.Antonyms),
            RelatedPhrases = StringHelper.NormalizeList(request.RelatedPhrases),
        };

        await _unitOfWork.Cards.AddAsync(card);
        await SyncVocabularySentencesAsync(cardId, request.Sentences, currentUserId);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.VocabularyMessage.CARD_NOT_FOUND, 404);

        return created.ToDetailResponse(new List<UserCardNote>(), currentUserId);
    }

    public async Task<VocabularyDetailResponse> UpdateAsync(string cardId, UpdateVocabularyCardRequest request, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Vocab)
            throw new AppException(MessageConstants.VocabularyMessage.CARD_NOT_FOUND, 404);

        var detail = await _unitOfWork.VocabularyDetails.GetByIdAsync(cardId);
        if (detail == null)
            throw new AppException(MessageConstants.VocabularyMessage.DETAIL_NOT_FOUND, 404);

        var writing = request.Writing.Trim();
        var reading = StringHelper.NormalizeOptional(request.Reading);
        var synthesisText = ResolveVocabularySynthesisText(writing, reading);

        var audioResult = await AzureTtsHelper.SynthesizeAndUploadAsync(
            _ttsService,
            _fileUploadService,
            synthesisText,
            currentUserId,
            $"vocab_{cardId}.mp3",
            MessageConstants.VocabularyMessage.AUDIO_SYNTHESIS_FAILED,
            _logger);

        var finalPitchPattern = request.PitchPattern;

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Writing = writing;
        detail.Reading = reading;
        detail.PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern);
        detail.AudioUrl = audioResult.AudioUrl;
        detail.WordType = EnumParsingHelper.ParseNullable<WordType>(request.WordType);
        detail.Meanings = VocabularyHelper.MapMeaningItems(request.Meanings);
        detail.Synonyms = StringHelper.NormalizeList(request.Synonyms);
        detail.Antonyms = StringHelper.NormalizeList(request.Antonyms);
        detail.RelatedPhrases = StringHelper.NormalizeList(request.RelatedPhrases);

        await SyncVocabularySentencesAsync(cardId, request.Sentences, currentUserId);
        _unitOfWork.Cards.UpdateAsync(card);
        _unitOfWork.VocabularyDetails.UpdateAsync(detail);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.VocabularyMessage.CARD_NOT_FOUND, 404);

        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return updated.ToDetailResponse(notes, currentUserId);
    }

    public async Task<bool> SoftDeleteAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Vocab)
            throw new AppException(MessageConstants.VocabularyMessage.CARD_NOT_FOUND, 404);

        card.Status = PublishStatus.Archived;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new AppException(MessageConstants.VocabularyMessage.READ_FORBIDDEN, 401);
    }

    private static string ResolveVocabularySynthesisText(string writing, string? reading)
    {
        return string.IsNullOrWhiteSpace(reading) ? writing : reading;
    }

    private async Task SyncVocabularySentencesAsync(
        string cardId,
        List<VocabularySentenceUpsertRequest> requests,
        string currentUserId)
    {
        var existingLinks = await _unitOfWork.CardSentences.GetByCardIdAsync(cardId);
        var keptSentenceIds = new HashSet<string>();

        foreach (var request in requests)
        {
            var sentence = await UpsertVocabularySentenceAsync(request, currentUserId);
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

    private async Task<Sentence> UpsertVocabularySentenceAsync(
        VocabularySentenceUpsertRequest request,
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
