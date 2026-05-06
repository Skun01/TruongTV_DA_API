using Domain.Constants;

namespace Infrastructure.InternalServices;

internal static class AiGenerationResponseParser
{
    public static string ExtractJson(string content, string errorCode = MessageConstants.AiQuestionMessage.GENERATION_FAILED)
    {
        var trimmed = content.Trim();

        if (trimmed.StartsWith("```"))
        {
            var startIdx = trimmed.IndexOf('\n');
            if (startIdx == -1) return trimmed;

            var endIdx = trimmed.LastIndexOf("```");
            if (endIdx <= startIdx) return trimmed;

            return trimmed.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();
        }

        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            return trimmed;

        var jsonStart = trimmed.IndexOf('{');
        if (jsonStart == -1)
            jsonStart = trimmed.IndexOf('[');

        if (jsonStart == -1)
            throw new ApplicationException(errorCode);

        return trimmed.Substring(jsonStart);
    }
}
