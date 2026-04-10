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

namespace Application.Services;

public class VocabularyDetailService : IVocabularyDetailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoicevoxService _voicevoxService;

    public VocabularyDetailService(IUnitOfWork unitOfWork, IVoicevoxService voicevoxService)
    {
        _unitOfWork = unitOfWork;
        _voicevoxService = voicevoxService;
    }

    public async Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string? currentUserId)
    {
        var card = await _unitOfWork.Cards.GetVocabularyDetailByIdAsync(cardId);
        if (card == null || card.VocabularyDetail == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

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
        var template = new ImportVocabularyRequest
        {
            Items = new List<ImportVocabularyItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Mode = "create",
                    Title = "食べる",
                    Summary = "Động từ ăn",
                    Level = "N5",
                    Tags = new List<string> { "verb", "daily-life" },
                    Status = "Draft",
                    Writing = "食べる",
                    Reading = "たべる",
                    PitchPattern = new List<int> { 0, 1, 0 },
                    SpeakerId = 3,
                    WordType = "Native",
                    Meanings = new List<VocabularyMeaningRequest>
                    {
                        new()
                        {
                            PartOfSpeech = "VerbRu",
                            Definitions = new List<string> { "ăn", "dùng bữa" },
                        },
                    },
                    Synonyms = new List<string> { "食事する" },
                    Antonyms = new List<string>(),
                    RelatedPhrases = new List<string> { "ご飯を食べる" },
                    Sentences = new List<VocabularySentenceUpsertRequest>
                    {
                        new()
                        {
                            Text = "毎朝パンを食べる。",
                            Meaning = "Mỗi sáng tôi ăn bánh mì.",
                            SpeakerId = 3,
                            Level = "N5",
                        },
                    },
                },
            },
        };

        return Task.FromResult(template);
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
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var previewItems = new List<VocabularyImportPreviewItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var previewItem = new VocabularyImportPreviewItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Mode = NormalizeMode(item.Mode),
                ExistingCardId = StringHelper.NormalizeOptional(item.ExistingCardId),
                Title = item.Title?.Trim() ?? string.Empty,
                Writing = item.Writing?.Trim() ?? string.Empty,
            };

            await ValidateImportItemAsync(item, previewItem);
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
            return BuildBlockedCommitResponse(preview);

        var commitItems = new List<VocabularyImportCommitItemResponse>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var mode = NormalizeMode(item.Mode);
            var existingCardId = StringHelper.NormalizeOptional(item.ExistingCardId);
            var commitItem = new VocabularyImportCommitItemResponse
            {
                RowNumber = item.RowNumber.GetValueOrDefault(index + 1),
                Mode = mode,
                ExistingCardId = existingCardId,
                Title = item.Title?.Trim() ?? string.Empty,
                Writing = item.Writing?.Trim() ?? string.Empty,
            };

            try
            {
                VocabularyDetailResponse result;

                if (mode == "upsert")
                {
                    result = await UpdateAsync(existingCardId!, item.ToUpdateRequest(), currentUserId);
                    commitItem.Action = "updated";
                }
                else
                {
                    result = await CreateAsync(item.ToCreateRequest(), currentUserId);
                    commitItem.Action = "created";
                }

                commitItem.IsSuccess = true;
                commitItem.CardId = result.Id;
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
        var synthesisResult = await _voicevoxService.SynthesizeAsync(synthesisText, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        var finalPitchPattern = (request.PitchPattern == null || request.PitchPattern.Count == 0)
            ? synthesisResult.PitchPattern
            : request.PitchPattern;

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
            AudioUrl = synthesisResult.AudioUrl,
            SpeakerId = synthesisResult.SpeakerId,
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
            ?? throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        return created.ToDetailResponse(new List<UserCardNote>(), currentUserId);
    }

    public async Task<VocabularyDetailResponse> UpdateAsync(string cardId, UpdateVocabularyCardRequest request, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var detail = await _unitOfWork.VocabularyDetails.GetByIdAsync(cardId);
        if (detail == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var writing = request.Writing.Trim();
        var reading = StringHelper.NormalizeOptional(request.Reading);
        var synthesisText = ResolveVocabularySynthesisText(writing, reading);
        var synthesisResult = await _voicevoxService.SynthesizeAsync(synthesisText, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        var finalPitchPattern = (request.PitchPattern == null || request.PitchPattern.Count == 0)
            ? synthesisResult.PitchPattern
            : request.PitchPattern;

        card.Title = request.Title.Trim();
        card.Summary = request.Summary.Trim();
        card.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        card.Tags = StringHelper.NormalizeList(request.Tags);
        card.Status = EnumParsingHelper.ParseNullable<PublishStatus>(request.Status) ?? card.Status;
        card.UpdatedAt = DateTime.UtcNow;

        detail.Writing = writing;
        detail.Reading = reading;
        detail.PitchAccent = VocabularyHelper.SerializePitchPattern(finalPitchPattern);
        detail.AudioUrl = synthesisResult.AudioUrl;
        detail.SpeakerId = synthesisResult.SpeakerId;
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
            ?? throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var notes = await _unitOfWork.UserCardNotes.GetByCardIdWithRelationsAsync(cardId);
        return updated.ToDetailResponse(notes, currentUserId);
    }

    public async Task<bool> SoftDeleteAsync(string cardId, string currentUserId)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId);
        if (card == null || card.CardType != CardType.Vocab)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        card.Status = PublishStatus.Archived;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static void EnsureCardReadable(Card card, string? currentUserId)
    {
        if (card.Status != PublishStatus.Published && (string.IsNullOrWhiteSpace(currentUserId) || card.CreatedBy != currentUserId))
            throw new ApplicationException(MessageConstants.CommonMessage.UNAUTHORIZED);
    }

    private static string ResolveVocabularySynthesisText(string writing, string? reading)
    {
        return string.IsNullOrWhiteSpace(reading) ? writing : reading;
    }

    private static VocabularyImportCommitResponse BuildBlockedCommitResponse(VocabularyImportPreviewResponse preview)
    {
        var items = preview.Items.Select(item => new VocabularyImportCommitItemResponse
        {
            RowNumber = item.RowNumber,
            Mode = item.Mode,
            ExistingCardId = item.ExistingCardId,
            Title = item.Title,
            Writing = item.Writing,
            IsSuccess = false,
            Action = "skipped",
            Errors = item.IsValid
                ? new List<string> { "Batch contains invalid items. Fix preview errors before commit." }
                : item.Errors.ToList(),
        }).ToList();

        return new VocabularyImportCommitResponse
        {
            TotalItems = preview.TotalItems,
            SuccessfulItems = 0,
            FailedItems = items.Count,
            HasValidationErrors = true,
            Items = items,
        };
    }

    private async Task ValidateImportItemAsync(
        ImportVocabularyItemRequest item,
        VocabularyImportPreviewItemResponse previewItem)
    {
        if (previewItem.RowNumber <= 0)
            previewItem.Errors.Add("rowNumber must be greater than 0.");

        if (previewItem.Mode is not ("create" or "upsert"))
            previewItem.Errors.Add("mode must be either 'create' or 'upsert'.");

        if (previewItem.Mode == "upsert")
        {
            if (string.IsNullOrWhiteSpace(previewItem.ExistingCardId))
            {
                previewItem.Errors.Add("existingCardId is required when mode is 'upsert'.");
            }
            else
            {
                var existingCard = await _unitOfWork.Cards.GetByIdAsync(previewItem.ExistingCardId);
                if (existingCard == null || existingCard.CardType != CardType.Vocab)
                    previewItem.Errors.Add("existingCardId does not match an existing vocabulary card.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(previewItem.ExistingCardId))
        {
            previewItem.Warnings.Add("existingCardId is ignored when mode is 'create'.");
        }

        ValidateRequiredText(item.Title, "title", 200, previewItem.Errors);
        ValidateRequiredText(item.Summary, "summary", 2000, previewItem.Errors);
        ValidateRequiredText(item.Writing, "writing", 200, previewItem.Errors);
        ValidateOptionalText(item.Reading, "reading", 200, previewItem.Errors);
        ValidateOptionalText(item.Level, "level", 10, previewItem.Errors);
        ValidateOptionalText(item.Status, "status", 20, previewItem.Errors);
        ValidateOptionalText(item.WordType, "wordType", 50, previewItem.Errors);

        ValidateOptionalEnum<JlptLevel>(item.Level, "level", previewItem.Errors);
        ValidateOptionalEnum<PublishStatus>(item.Status, "status", previewItem.Errors);
        ValidateOptionalEnum<WordType>(item.WordType, "wordType", previewItem.Errors);

        if (item.SpeakerId.HasValue && item.SpeakerId.Value <= 0)
            previewItem.Errors.Add("speakerId must be greater than 0.");

        ValidateListItems(item.Tags, "tags", 20, 100, previewItem.Errors);
        ValidateListItems(item.Synonyms, "synonyms", null, 200, previewItem.Errors);
        ValidateListItems(item.Antonyms, "antonyms", null, 200, previewItem.Errors);
        ValidateListItems(item.RelatedPhrases, "relatedPhrases", null, 200, previewItem.Errors);

        ValidateMeanings(item.Meanings, previewItem.Errors);
        ValidateSentences(item.Sentences, previewItem.Errors);
    }

    private static void ValidateMeanings(List<VocabularyMeaningRequest>? meanings, List<string> errors)
    {
        if (meanings == null || meanings.Count == 0)
        {
            errors.Add("meanings must contain at least one item.");
            return;
        }

        for (var index = 0; index < meanings.Count; index++)
        {
            var meaning = meanings[index];
            var path = $"meanings[{index}]";

            ValidateRequiredText(meaning.PartOfSpeech, $"{path}.partOfSpeech", 100, errors);
            ValidateOptionalEnum<PartOfSpeech>(meaning.PartOfSpeech, $"{path}.partOfSpeech", errors, required: true);

            if (meaning.Definitions == null || meaning.Definitions.Count == 0)
            {
                errors.Add($"{path}.definitions must contain at least one item.");
                continue;
            }

            for (var definitionIndex = 0; definitionIndex < meaning.Definitions.Count; definitionIndex++)
            {
                ValidateRequiredText(
                    meaning.Definitions[definitionIndex],
                    $"{path}.definitions[{definitionIndex}]",
                    500,
                    errors);
            }
        }
    }

    private static void ValidateSentences(List<VocabularySentenceUpsertRequest>? sentences, List<string> errors)
    {
        if (sentences == null)
            return;

        if (sentences.Count > 20)
            errors.Add("sentences cannot exceed 20 items.");

        for (var index = 0; index < sentences.Count; index++)
        {
            var sentence = sentences[index];
            var path = $"sentences[{index}]";

            ValidateOptionalText(sentence.Id, $"{path}.id", 100, errors);
            ValidateRequiredText(sentence.Text, $"{path}.text", 500, errors);
            ValidateRequiredText(sentence.Meaning, $"{path}.meaning", 500, errors);
            ValidateOptionalText(sentence.Level, $"{path}.level", 10, errors);
            ValidateOptionalEnum<JlptLevel>(sentence.Level, $"{path}.level", errors);

            if (sentence.SpeakerId.HasValue && sentence.SpeakerId.Value <= 0)
                errors.Add($"{path}.speakerId must be greater than 0.");
        }
    }

    private static void ValidateListItems(
        List<string>? values,
        string fieldName,
        int? maxItems,
        int maxLength,
        List<string> errors)
    {
        if (values == null)
            return;

        if (maxItems.HasValue && values.Count > maxItems.Value)
            errors.Add($"{fieldName} cannot exceed {maxItems.Value} items.");

        for (var index = 0; index < values.Count; index++)
        {
            ValidateRequiredText(values[index], $"{fieldName}[{index}]", maxLength, errors);
        }
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
        List<string> errors,
        bool required = false) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
                errors.Add($"{fieldName} is required.");

            return;
        }

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add($"{fieldName} is invalid.");
    }

    private static string NormalizeMode(string? mode)
    {
        return string.IsNullOrWhiteSpace(mode) ? "create" : mode.Trim().ToLowerInvariant();
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

            if (existingLinks.Any(link => link.SentenceId == sentence.Id))
                continue;

            await _unitOfWork.CardSentences.AddAsync(new CardSentence
            {
                CardId = cardId,
                SentenceId = sentence.Id,
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
        var synthesisResult = await _voicevoxService.SynthesizeAsync(text, request.SpeakerId)
            ?? throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            var sentence = new Sentence
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Meaning = request.Meaning.Trim(),
                AudioUrl = synthesisResult.AudioUrl,
                SpeakerId = synthesisResult.SpeakerId,
                Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level),
                CreatedBy = currentUserId,
            };

            await _unitOfWork.Sentences.AddAsync(sentence);
            return sentence;
        }

        var existingSentence = await _unitOfWork.Sentences.GetByIdAsync(request.Id);
        if (existingSentence == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        existingSentence.Text = text;
        existingSentence.Meaning = request.Meaning.Trim();
        existingSentence.AudioUrl = synthesisResult.AudioUrl;
        existingSentence.SpeakerId = synthesisResult.SpeakerId;
        existingSentence.Level = EnumParsingHelper.ParseNullable<JlptLevel>(request.Level);
        existingSentence.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sentences.UpdateAsync(existingSentence);
        return existingSentence;
    }
}
