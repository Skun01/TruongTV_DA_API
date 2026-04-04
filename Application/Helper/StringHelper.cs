namespace Application.Helper;

public static class StringHelper
{
    public static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    public static List<string> NormalizeList(List<string>? values)
    {
        if (values == null || values.Count == 0)
            return new List<string>();

        return values
            .Select(v => v?.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
