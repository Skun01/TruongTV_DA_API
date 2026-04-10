using System.Text.RegularExpressions;
using Application.Common;
using Domain.Constants;

namespace Application.Helper;

public static class GrammarMarkdownHelper
{
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex CustomTokenRegex = new(@"\{(/?[a-z]+)\}", RegexOptions.Compiled);
    private static readonly HashSet<string> AllowedCustomTags = new(StringComparer.Ordinal)
    {
        "u",
        "red",
        "blue",
        "green",
        "yellow",
        "orange",
        "purple",
        "gray",
    };

    public static string NormalizeRequired(string value, string fieldPath, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
            throw BuildInvalid(fieldPath, "Value is required.");

        ValidateMarkdownSubset(normalized, fieldPath, maxLength);
        return normalized;
    }

    public static string? NormalizeOptional(string? value, string fieldPath, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        ValidateMarkdownSubset(normalized, fieldPath, maxLength);
        return normalized;
    }

    private static void ValidateMarkdownSubset(string value, string fieldPath, int maxLength)
    {
        if (value.Length > maxLength)
            throw BuildInvalid(fieldPath, $"Value exceeds max length {maxLength}.");

        if (HtmlTagRegex.IsMatch(value))
            throw BuildInvalid(fieldPath, "Raw HTML is not allowed.");

        ValidateCustomTokens(value, fieldPath);
    }

    private static void ValidateCustomTokens(string value, string fieldPath)
    {
        var stack = new Stack<string>();
        var matches = CustomTokenRegex.Matches(value);

        foreach (Match match in matches)
        {
            var tokenValue = match.Groups[1].Value;
            var isClosing = tokenValue.StartsWith("/", StringComparison.Ordinal);
            var tag = isClosing ? tokenValue[1..] : tokenValue;

            if (!AllowedCustomTags.Contains(tag))
                throw BuildInvalid(fieldPath, $"Unsupported custom tag: {tag}.");

            if (!isClosing)
            {
                stack.Push(tag);
                continue;
            }

            if (stack.Count == 0 || stack.Pop() != tag)
                throw BuildInvalid(fieldPath, $"Invalid custom tag order: {match.Value}.");
        }

        if (stack.Count > 0)
            throw BuildInvalid(fieldPath, "Custom tag is not closed.");

        var withoutKnownTokens = CustomTokenRegex.Replace(value, string.Empty);
        if (withoutKnownTokens.Contains('{') || withoutKnownTokens.Contains('}'))
            throw BuildInvalid(fieldPath, "Invalid custom token syntax.");
    }

    private static AppException BuildInvalid(string fieldPath, string reason)
    {
        return new AppException(
            MessageConstants.GrammarMessage.INVALID_RICH_TEXT,
            400,
            details: new { field = fieldPath, reason });
    }
}
