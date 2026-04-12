using Application.DTOs.Kanji;
using Application.IRepositories;
using Domain.Constants;
using Domain.Enums;

namespace Application.Helper;

public static class KanjiImportHelper
{
    public static ImportKanjiRequest CreateTemplate()
    {
        return new ImportKanjiRequest
        {
            Items = new List<ImportKanjiItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Title = "明",
                    Summary = "Kanji chỉ sự sáng, rõ ràng.",
                    Level = "N5",
                    Tags = new List<string> { "kanji", "co-ban" },
                    Status = "Draft",
                    Kanji = "明",
                    StrokeCount = 8,
                    StrokeOrderUrl = "https://example.com/kanji/mei-stroke-order.gif",
                    Onyomi = new List<string> { "メイ", "ミョウ" },
                    Kunyomi = new List<string> { "あ.かり", "あか.るい" },
                    HanViet = "minh",
                    MeaningVi = "sáng, rõ",
                    Radicals = new List<KanjiRadicalUpsertRequest>
                    {
                        new() { Character = "日", MeaningVi = "mặt trời" },
                        new() { Character = "月", MeaningVi = "mặt trăng" },
                    },
                },
            },
        };
    }

    public static KanjiImportCommitResponse BuildBlockedCommitResponse(KanjiImportPreviewResponse preview)
    {
        var items = preview.Items.Select(item => new KanjiImportCommitItemResponse
        {
            RowNumber = item.RowNumber,
            Title = item.Title,
            Kanji = item.Kanji,
            IsSuccess = false,
            Action = "skipped",
            Errors = item.IsValid
                ? new List<string> { MessageConstants.KanjiMessage.IMPORT_BATCH_HAS_ERRORS }
                : item.Errors.ToList(),
        }).ToList();

        return new KanjiImportCommitResponse
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
        ImportKanjiItemRequest item,
        KanjiImportPreviewItemResponse previewItem,
        HashSet<string> batchKanjiSet)
    {
        if (previewItem.RowNumber <= 0)
            previewItem.Errors.Add(MessageConstants.KanjiMessage.IMPORT_ROW_NUMBER_INVALID);

        ValidateRequiredText(item.Title, "title", 200, previewItem.Errors);
        ValidateRequiredText(item.Summary, "summary", 2000, previewItem.Errors);
        ValidateRequiredText(item.Kanji, "kanji", 20, previewItem.Errors);
        ValidateRequiredText(item.MeaningVi, "meaningVi", 1000, previewItem.Errors);
        ValidateOptionalText(item.Level, "level", 10, previewItem.Errors);
        ValidateOptionalText(item.Status, "status", 20, previewItem.Errors);
        ValidateOptionalText(item.StrokeOrderUrl, "strokeOrderUrl", 2000, previewItem.Errors);
        ValidateOptionalText(item.HanViet, "hanViet", 200, previewItem.Errors);

        ValidateOptionalEnum<JlptLevel>(item.Level, "level", previewItem.Errors);
        ValidateOptionalEnum<PublishStatus>(item.Status, "status", previewItem.Errors);

        if (item.StrokeCount <= 0)
            previewItem.Errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_FIELD_INVALID, "strokeCount"));

        ValidateListItems(item.Tags, "tags", 20, 100, previewItem.Errors);
        ValidateListItems(item.Onyomi, "onyomi", 20, 100, previewItem.Errors);
        ValidateListItems(item.Kunyomi, "kunyomi", 20, 100, previewItem.Errors);
        ValidateRadicals(item.Radicals, previewItem.Errors);

        if (previewItem.Errors.Count > 0)
            return;

        var normalizedKanji = item.Kanji.Trim();
        if (!batchKanjiSet.Add(normalizedKanji))
            previewItem.Errors.Add(MessageConstants.KanjiMessage.IMPORT_DUPLICATE_KANJI_IN_BATCH);

        if (await unitOfWork.Cards.ExistsKanjiByCharacterAsync(normalizedKanji))
            previewItem.Errors.Add(MessageConstants.KanjiMessage.IMPORT_KANJI_ALREADY_EXISTS);
    }

    private static void ValidateRadicals(List<KanjiRadicalUpsertRequest>? radicals, List<string> errors)
    {
        if (radicals == null || radicals.Count == 0)
        {
            errors.Add(MessageConstants.KanjiMessage.IMPORT_RADICALS_REQUIRED);
            return;
        }

        if (radicals.Count > 30)
            errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_LIST_TOO_MANY_ITEMS, "radicals"));

        var radicalSet = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < radicals.Count; i++)
        {
            var radical = radicals[i];
            var path = $"radicals[{i}]";
            ValidateRequiredText(radical.Character, $"{path}.character", 20, errors);
            ValidateRequiredText(radical.MeaningVi, $"{path}.meaningVi", 500, errors);

            if (!string.IsNullOrWhiteSpace(radical.Character) && !radicalSet.Add(radical.Character.Trim()))
                errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_DUPLICATE_RADICAL_IN_ITEM, $"{path}.character"));
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
            errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_LIST_TOO_MANY_ITEMS, fieldName));

        for (var index = 0; index < values.Count; index++)
        {
            ValidateRequiredText(values[index], $"{fieldName}[{index}]", maxLength, errors);
        }
    }

    private static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_FIELD_REQUIRED, fieldName));
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalEnum<TEnum>(
        string? value,
        string fieldName,
        List<string> errors) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add(BuildFieldCode(MessageConstants.KanjiMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static string BuildFieldCode(string code, string fieldName)
    {
        return $"{code}:{fieldName}";
    }
}
