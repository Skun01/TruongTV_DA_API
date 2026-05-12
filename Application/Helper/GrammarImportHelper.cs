using Application.Common;
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
        guide.FieldNotes["status"] = "Neu bo trong khi import, he thong mac dinh Published.";
        guide.FieldNotes["structures"] = "Bat buoc, it nhat 1 phan tu.";
        guide.FieldNotes["structures[].pattern"] = "Bat buoc, toi da 1000 ky tu.";
        guide.FieldNotes["structures[].annotations"] = "Toi da 20 annotation, key chi gom chu, so, '_' hoac '-'.";
        guide.FieldNotes["relations[].relatedId"] = "Phai la cardId cua grammar da ton tai.";
        guide.FieldNotes["relations[].relationType"] = "Bat buoc khi co relation.";
        guide.FieldNotes["resources[].url"] = "Bat buoc la URL tuyet doi hop le.";
        guide.FieldNotes["sentences[].id"] = "Khong truyen khi import tao moi tu template.";
        guide.FieldNotes["sentences[].position"] = "Bat buoc > 0 de xac dinh thu tu cau trong card.";
        guide.FieldNotes["sentences[].blankWord"] = "Tu/cum tu bi an trong bai tap dien tu (tuy chon).";
        guide.FieldNotes["sentences[].hint"] = "Go y cho cau hoi dien tu (tuy chon).";
        guide.FieldNotes["sentences[].answerList"] = "Danh sach dap an chap nhan (tuy chon).";

        return new ImportGrammarRequest
        {
            Guide = guide,
            Items = new List<ImportGrammarItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Title = "~ながら",
                    Summary = "Vua lam A vua lam B.",
                    Level = "N4",
                    Tags = new List<string> { "grammar", "simultaneous" },
                    Status = "Published",
                    Structures = new List<GrammarStructureRequest>
                    {
                        new()
                        {
                            Pattern = "V1(1) + ながら + V2(2)",
                            Annotations = new Dictionary<string, string>
                            {
                                { "1", "Hanh dong phu dien ra dong thoi." },
                                { "2", "Hanh dong chinh." },
                            },
                        },
                    },
                    Explanation = "Dung khi chu the vua lam hanh dong A vua lam hanh dong B.",
                    Caution = "Hai hanh dong can cung chu the.",
                    Register = "Standard",
                    AlternateForms = new List<string> { "~つつ" },
                    Relations = new List<GrammarRelationUpsertRequest>(),
                    Resources = new List<GrammarResourceUpsertRequest>
                    {
                        new()
                        {
                            Title = "Bai giang mau",
                            Url = "https://example.com/grammar/nagara",
                        },
                    },
                    Sentences = new List<GrammarSentenceUpsertRequest>
                    {
                        new()
                        {
                            Text = "音楽を聞きながら勉強します。",
                            Meaning = "Toi vua nghe nhac vua hoc.",
                            Position = 1,
                            Level = "N4",
                            BlankWord = "聞きながら",
                            Hint = "Mau vua lam A vua lam B.",
                            AnswerList = new List<string> { "聞きながら" },
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
        ValidateMarkdownOptional(item.Explanation, "explanation", 10000, previewItem.Errors);
        ValidateMarkdownOptional(item.Caution, "caution", 5000, previewItem.Errors);

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

        for (var i = 0; i < structures.Count; i++)
        {
            var item = structures[i];
            var path = $"structures[{i}]";

            ValidateMarkdownRequired(item.Pattern, $"{path}.pattern", 1000, errors);

            if (item.Annotations == null)
                continue;

            if (item.Annotations.Count > 20)
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_LIST_TOO_MANY_ITEMS, $"{path}.annotations"));

            foreach (var annotation in item.Annotations)
            {
                var key = annotation.Key?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key)
                    || key.Length > 20
                    || key.Any(ch => !char.IsLetterOrDigit(ch) && ch != '_' && ch != '-'))
                {
                    errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_INVALID, $"{path}.annotations"));
                    break;
                }

                ValidateMarkdownRequired(annotation.Value, $"{path}.annotations.{key}", 1000, errors);
            }
        }
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

            if (!string.IsNullOrWhiteSpace(item.Url)
                && !Uri.TryCreate(item.Url.Trim(), UriKind.Absolute, out _))
            {
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_INVALID, $"resources[{i}].url"));
            }
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

            if (sentence.Position <= 0)
                errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.IMPORT_FIELD_INVALID, $"{path}.position"));

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

    private static void ValidateMarkdownRequired(string? value, string fieldName, int maxLength, List<string> errors)
    {
        try
        {
            GrammarMarkdownHelper.NormalizeRequired(value ?? string.Empty, fieldName, maxLength);
        }
        catch (AppException)
        {
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.INVALID_RICH_TEXT, fieldName));
        }
    }

    private static void ValidateMarkdownOptional(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        try
        {
            GrammarMarkdownHelper.NormalizeOptional(value, fieldName, maxLength);
        }
        catch (AppException)
        {
            errors.Add(BuildFieldCode(MessageConstants.GrammarMessage.INVALID_RICH_TEXT, fieldName));
        }
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
