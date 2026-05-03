using Application.DTOs.Sentences;
using Domain.Constants;
using Domain.Enums;

namespace Application.Helper;

public static class SentenceImportHelper
{
    public static ImportSentenceRequest CreateTemplate()
    {
        var guide = ImportTemplateGuideHelper.CreateBaseGuide();
        guide.AllowedValues["level"] = ImportTemplateGuideHelper.EnumValues<JlptLevel>();

        return new ImportSentenceRequest
        {
            Guide = guide,
            Items = new List<ImportSentenceItemRequest>
            {
                new()
                {
                    RowNumber = 1,
                    Text = "日本へ行きたいです。",
                    Meaning = "Tôi muốn đi Nhật.",
                    Level = "N5",
                },
            },
        };
    }

    public static SentenceImportCommitResponse BuildBlockedCommitResponse(SentenceImportPreviewResponse preview)
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

    public static void ValidateImportItem(
        ImportSentenceItemRequest item,
        SentenceImportPreviewItemResponse previewItem)
    {
        if (previewItem.RowNumber <= 0)
            previewItem.Errors.Add(MessageConstants.SentenceMessage.IMPORT_ROW_NUMBER_INVALID);

        ValidateRequiredText(item.Text, "text", 500, previewItem.Errors);
        ValidateRequiredText(item.Meaning, "meaning", 500, previewItem.Errors);
        ValidateOptionalText(item.Level, "level", 10, previewItem.Errors);
        ValidateOptionalEnum<JlptLevel>(item.Level, "level", previewItem.Errors);
    }

    private static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(BuildFieldCode(MessageConstants.SentenceMessage.IMPORT_FIELD_REQUIRED, fieldName));
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.SentenceMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.SentenceMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalEnum<TEnum>(
        string? value,
        string fieldName,
        List<string> errors) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add(BuildFieldCode(MessageConstants.SentenceMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static string BuildFieldCode(string code, string fieldName)
    {
        return $"{code}:{fieldName}";
    }
}
