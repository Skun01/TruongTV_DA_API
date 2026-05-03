using Application.DTOs.Vocabulary;
using Application.IRepositories;
using Domain.Constants;
using Domain.Enums;

namespace Application.Helper;

public static class VocabularyImportHelper
{
    public static ImportVocabularyRequest CreateTemplate()
    {
        var guide = ImportTemplateGuideHelper.CreateBaseGuide();
        guide.AllowedValues["level"] = ImportTemplateGuideHelper.EnumValues<JlptLevel>();
        guide.AllowedValues["status"] = ImportTemplateGuideHelper.EnumValues<PublishStatus>();
        guide.AllowedValues["wordType"] = ImportTemplateGuideHelper.EnumValues<WordType>();
        guide.AllowedValues["meanings[].partOfSpeech"] = ImportTemplateGuideHelper.EnumValues<PartOfSpeech>();
        guide.AllowedValues["sentences[].level"] = ImportTemplateGuideHelper.EnumValues<JlptLevel>();
        guide.FieldNotes["status"] = "Nếu bỏ trống khi import, hệ thống mặc định Published.";
        guide.FieldNotes["meanings[].partOfSpeech"] = "Bắt buộc, phải là enum hợp lệ.";
        guide.FieldNotes["pitchPattern"] = "Mảng số nguyên, ví dụ [0,1,0]. Admin nhập thủ công.";
        guide.FieldNotes["sentences[].id"] = "Không truyền khi import tạo mới từ template.";
        guide.FieldNotes["sentences[].position"] = "Bắt buộc > 0 để xác định thứ tự câu trong card.";
        guide.FieldNotes["sentences[].blankWord"] = "Từ/cụm từ bị ẩn trong bài tập điền từ (tuỳ chọn).";
        guide.FieldNotes["sentences[].hint"] = "Gợi ý cho câu hỏi điền từ (tuỳ chọn).";
        guide.FieldNotes["sentences[].answerList"] = "Danh sách đáp án chấp nhận (tuỳ chọn).";

        return new ImportVocabularyRequest
        {
            Guide = guide,
            Items = new List<ImportVocabularyItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Title = "食べる",
                    Summary = "Động từ ăn",
                    Level = "N5",
                    Tags = new List<string> { "verb", "daily-life" },
                    Status = "Published",
                    Writing = "食べる",
                    Reading = "たべる",
                    PitchPattern = new List<int> { 0, 1, 0 },
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
                            Position = 1,
                            Level = "N5",
                            BlankWord = "食べる",
                            Hint = "Động từ chính trong câu.",
                            AnswerList = new List<string> { "食べる", "たべる" },
                        },
                    },
                },
            },
        };
    }

    public static VocabularyImportCommitResponse BuildBlockedCommitResponse(VocabularyImportPreviewResponse preview)
    {
        var items = preview.Items.Select(item => new VocabularyImportCommitItemResponse
        {
            RowNumber = item.RowNumber,
            Title = item.Title,
            Writing = item.Writing,
            IsSuccess = false,
            Action = "skipped",
            Errors = item.IsValid
                ? new List<string> { MessageConstants.VocabularyMessage.IMPORT_BATCH_HAS_ERRORS }
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

    public static async Task ValidateImportItemAsync(
        IUnitOfWork unitOfWork,
        ImportVocabularyItemRequest item,
        VocabularyImportPreviewItemResponse previewItem,
        HashSet<string> batchWritingSet)
    {
        if (previewItem.RowNumber <= 0)
            previewItem.Errors.Add(MessageConstants.VocabularyMessage.IMPORT_ROW_NUMBER_INVALID);

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

        ValidateListItems(item.Tags, "tags", 20, 100, previewItem.Errors);
        ValidateListItems(item.Synonyms, "synonyms", null, 200, previewItem.Errors);
        ValidateListItems(item.Antonyms, "antonyms", null, 200, previewItem.Errors);
        ValidateListItems(item.RelatedPhrases, "relatedPhrases", null, 200, previewItem.Errors);

        ValidateMeanings(item.Meanings, previewItem.Errors);
        ValidateSentences(item.Sentences, previewItem.Errors);

        if (previewItem.Errors.Count > 0)
            return;

        var normalizedWriting = item.Writing.Trim();

        if (!batchWritingSet.Add(normalizedWriting))
            previewItem.Errors.Add(MessageConstants.VocabularyMessage.IMPORT_DUPLICATE_WRITING_IN_BATCH);

        if (await unitOfWork.Cards.ExistsVocabularyByWritingAsync(normalizedWriting))
            previewItem.Errors.Add(MessageConstants.VocabularyMessage.IMPORT_WRITING_ALREADY_EXISTS);
    }

    private static void ValidateMeanings(List<VocabularyMeaningRequest>? meanings, List<string> errors)
    {
        if (meanings == null || meanings.Count == 0)
        {
            errors.Add(MessageConstants.VocabularyMessage.IMPORT_MEANINGS_REQUIRED);
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
                errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_DEFINITIONS_REQUIRED, $"{path}.definitions"));
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
            errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_SENTENCES_TOO_MANY, "sentences"));

        for (var index = 0; index < sentences.Count; index++)
        {
            var sentence = sentences[index];
            var path = $"sentences[{index}]";

            if (!string.IsNullOrWhiteSpace(sentence.Id))
                errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_SENTENCE_ID_NOT_ALLOWED, $"{path}.id"));

            if (sentence.Position <= 0)
                errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_FIELD_INVALID, $"{path}.position"));

            ValidateRequiredText(sentence.Text, $"{path}.text", 500, errors);
            ValidateRequiredText(sentence.Meaning, $"{path}.meaning", 500, errors);
            ValidateOptionalText(sentence.Level, $"{path}.level", 10, errors);
            ValidateOptionalEnum<JlptLevel>(sentence.Level, $"{path}.level", errors);
            ValidateOptionalText(sentence.BlankWord, $"{path}.blankWord", 200, errors);
            ValidateOptionalText(sentence.Hint, $"{path}.hint", 500, errors);
            ValidateAnswerList(sentence.AnswerList, $"{path}.answerList", errors);
        }
    }

    private static void ValidateAnswerList(List<string>? values, string fieldName, List<string> errors)
    {
        if (values == null)
            return;

        for (var index = 0; index < values.Count; index++)
        {
            ValidateRequiredText(values[index], $"{fieldName}[{index}]", 200, errors);
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
            errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_LIST_TOO_MANY_ITEMS, fieldName));

        for (var index = 0; index < values.Count; index++)
        {
            ValidateRequiredText(values[index], $"{fieldName}[{index}]", maxLength, errors);
        }
    }

    private static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_FIELD_REQUIRED, fieldName));
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_FIELD_TOO_LONG, fieldName));
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
                errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_FIELD_REQUIRED, fieldName));

            return;
        }

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add(BuildFieldCode(MessageConstants.VocabularyMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static string BuildFieldCode(string code, string fieldName)
    {
        return $"{code}:{fieldName}";
    }
}
