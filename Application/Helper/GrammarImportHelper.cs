using Application.DTOs.Grammar;
using Application.IRepositories;
using Domain.Constants;
using Domain.Enums;

namespace Application.Helper;

public static class GrammarImportHelper
{
    public static ImportGrammarRequest CreateTemplate()
    {
        var guide = ImportTemplateGuideHelper.CreateBaseGuide();
        guide.AllowedValues["level"] = ImportTemplateGuideHelper.EnumValues<JlptLevel>();
        guide.AllowedValues["status"] = ImportTemplateGuideHelper.EnumValues<PublishStatus>();
        guide.AllowedValues["register"] = ImportTemplateGuideHelper.EnumValues<RegisterType>();
        guide.AllowedValues["relations[].relationType"] = ImportTemplateGuideHelper.EnumValues<GrammarRelationType>();
        guide.AllowedValues["sentences[].level"] = ImportTemplateGuideHelper.EnumValues<JlptLevel>();
        guide.AllowedValues["sentences[].speakerId"] = ImportTemplateGuideHelper.RecommendedSpeakerIds();
        guide.FieldNotes["structures"] = "Bắt buộc, ít nhất 1 phần tử.";
        guide.FieldNotes["relations[].relatedId"] = "Phải là cardId của grammar đã tồn tại.";
        guide.FieldNotes["relations[].relationType"] = "Bắt buộc khi có relation.";
        guide.FieldNotes["sentences[].id"] = "Không truyền khi import tạo mới từ template.";

        return new ImportGrammarRequest
        {
            Guide = guide,
            Items = new List<ImportGrammarItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Title = "〜ながら",
                    Summary = "Vừa làm A vừa làm B.",
                    Level = "N4",
                    Tags = new List<string> { "grammar", "simultaneous" },
                    Status = "Draft",
                    Structures = new List<GrammarStructureRequest>
                    {
                        new()
                        {
                            Pattern = "V1(1) + ながら + V2(2)",
                            Annotations = new Dictionary<string, string>
                            {
                                { "1", "Hành động phụ diễn ra đồng thời." },
                                { "2", "Hành động chính." },
                            },
                        },
                    },
                    Explanation = "Dùng khi chủ thể vừa làm hành động A vừa làm hành động B.",
                    Caution = "Hai hành động cần cùng chủ thể.",
                    Register = "Standard",
                    AlternateForms = new List<string> { "〜つつ" },
                    Relations = new List<GrammarRelationUpsertRequest>(),
                    Resources = new List<GrammarResourceUpsertRequest>
                    {
                        new()
                        {
                            Title = "Bài giảng mẫu",
                            Url = "https://example.com/grammar/nagara",
                        },
                    },
                    Sentences = new List<GrammarSentenceUpsertRequest>
                    {
                        new()
                        {
                            Text = "音楽を聞きながら勉強します。",
                            Meaning = "Tôi vừa nghe nhạc vừa học.",
                            SpeakerId = 3,
                            Level = "N4",
                        },
                    },
                },
            },
        };
    }

    public static GrammarImportCommitResponse BuildBlockedCommitResponse(GrammarImportPreviewResponse preview)
    {
        var items = preview.Items.Select(item => new GrammarImportCommitItemResponse
        {
            RowNumber = item.RowNumber,
            Title = item.Title,
            IsSuccess = false,
            Action = "skipped",
            Errors = item.IsValid
                ? new List<string> { MessageConstants.GrammarMessage.IMPORT_BATCH_HAS_ERRORS }
                : item.Errors.ToList(),
        }).ToList();

        return new GrammarImportCommitResponse
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
        ImportGrammarItemRequest item,
        GrammarImportPreviewItemResponse previewItem)
    {
        if (previewItem.RowNumber <= 0)
            previewItem.Errors.Add(MessageConstants.GrammarMessage.IMPORT_ROW_NUMBER_INVALID);

        ValidateRequiredText(item.Title, "title", 200, previewItem.Errors);
        ValidateRequiredText(item.Summary, "summary", 2000, previewItem.Errors);
        ValidateOptionalText(item.Level, "level", 10, previewItem.Errors);
        ValidateOptionalText(item.Status, "status", 20, previewItem.Errors);
        ValidateOptionalText(item.Register, "register", 50, previewItem.Errors);

        ValidateOptionalEnum<JlptLevel>(item.Level, "level", previewItem.Errors);
        ValidateOptionalEnum<PublishStatus>(item.Status, "status", previewItem.Errors);
        ValidateOptionalEnum<RegisterType>(item.Register, "register", previewItem.Errors);

        ValidateListItems(item.Tags, "tags", 20, 100, previewItem.Errors);
        ValidateListItems(item.AlternateForms, "alternateForms", 20, 300, previewItem.Errors);

        ValidateStructures(item.Structures, previewItem.Errors);
        ValidateResources(item.Resources, previewItem.Errors);
        ValidateSentences(item.Sentences, previewItem.Errors);
        await ValidateRelationsAsync(unitOfWork, item.Relations, previewItem.Errors);
    }

    private static void ValidateStructures(List<GrammarStructureRequest>? structures, List<string> errors)
    {
        if (structures == null || structures.Count == 0)
        {
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_REQUIRED, "structures"));
            return;
        }

        if (structures.Count > 30)
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_LIST_TOO_MANY_ITEMS, "structures"));
    }

    private static void ValidateResources(List<GrammarResourceUpsertRequest>? resources, List<string> errors)
    {
        if (resources == null)
            return;

        if (resources.Count > 30)
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_LIST_TOO_MANY_ITEMS, "resources"));

        for (var i = 0; i < resources.Count; i++)
        {
            var item = resources[i];
            ValidateRequiredText(item.Title, $"resources[{i}].title", 300, errors);
            ValidateRequiredText(item.Url, $"resources[{i}].url", 2000, errors);
        }
    }

    private static async Task ValidateRelationsAsync(
        IUnitOfWork unitOfWork,
        List<GrammarRelationUpsertRequest>? relations,
        List<string> errors)
    {
        if (relations == null)
            return;

        if (relations.Count > 50)
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_LIST_TOO_MANY_ITEMS, "relations"));

        var relationSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < relations.Count; i++)
        {
            var relation = relations[i];
            var fieldPath = $"relations[{i}]";
            var localErrors = new List<string>();

            ValidateRequiredText(relation.RelatedId, $"{fieldPath}.relatedId", 50, localErrors);
            ValidateRequiredText(relation.RelationType, $"{fieldPath}.relationType", 50, localErrors);
            ValidateOptionalEnum<GrammarRelationType>(relation.RelationType, $"{fieldPath}.relationType", localErrors, true);

            if (localErrors.Count > 0)
            {
                errors.AddRange(localErrors);
                continue;
            }

            var dedupKey = $"{relation.RelatedId.Trim()}:{relation.RelationType.Trim()}";
            if (!relationSet.Add(dedupKey))
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_DUPLICATE_RELATION, fieldPath));

            var relatedCard = await unitOfWork.Cards.GetByIdAsync(relation.RelatedId.Trim());
            if (relatedCard == null || relatedCard.CardType != CardType.Grammar)
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_RELATED_GRAMMAR_NOT_FOUND, $"{fieldPath}.relatedId"));
        }
    }

    private static void ValidateSentences(List<GrammarSentenceUpsertRequest>? sentences, List<string> errors)
    {
        if (sentences == null)
            return;

        if (sentences.Count > 20)
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_SENTENCES_TOO_MANY, "sentences"));

        for (var index = 0; index < sentences.Count; index++)
        {
            var sentence = sentences[index];
            var path = $"sentences[{index}]";

            if (!string.IsNullOrWhiteSpace(sentence.Id))
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_SENTENCE_ID_NOT_ALLOWED, $"{path}.id"));

            ValidateRequiredText(sentence.Text, $"{path}.text", 500, errors);
            ValidateRequiredText(sentence.Meaning, $"{path}.meaning", 500, errors);
            ValidateOptionalText(sentence.Level, $"{path}.level", 10, errors);
            ValidateOptionalEnum<JlptLevel>(sentence.Level, $"{path}.level", errors);

            if (sentence.SpeakerId.HasValue && sentence.SpeakerId.Value <= 0)
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_SPEAKER_ID_INVALID, $"{path}.speakerId"));

            if (sentence.SpeakerId.HasValue && !VoicevoxConstants.RecommendedSpeakerIdSet.Contains(sentence.SpeakerId.Value))
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_SPEAKER_ID_NOT_SUPPORTED, $"{path}.speakerId"));
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
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_LIST_TOO_MANY_ITEMS, fieldName));

        for (var index = 0; index < values.Count; index++)
        {
            ValidateRequiredText(values[index], $"{fieldName}[{index}]", maxLength, errors);
        }
    }

    private static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_REQUIRED, fieldName));
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_TOO_LONG, fieldName));
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
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_REQUIRED, fieldName));

            return;
        }

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static string BuildFieldCode(string code, string fieldName)
    {
        return $"{code}:{fieldName}";
    }
}
