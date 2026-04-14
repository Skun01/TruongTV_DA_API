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

    public static List<string> ExtractDistinctKanjiCharacters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value
            .Trim()
            .Where(IsKanjiCharacter)
            .Select(character => character.ToString())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsKanjiCharacter(char character)
    {
        return (character >= '\u3400' && character <= '\u4DBF')
            || (character >= '\u4E00' && character <= '\u9FFF')
            || (character >= '\uF900' && character <= '\uFAFF');
    }
}
