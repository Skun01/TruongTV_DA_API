namespace Application.Helper;

public static class ImportValidationHelper
{
    public static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add($"{fieldName} must not exceed {maxLength} characters.");
    }

    public static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add($"{fieldName} must not exceed {maxLength} characters.");
    }

    public static void ValidateOptionalEnum<TEnum>(
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
}
