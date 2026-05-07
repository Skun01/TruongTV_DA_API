using System.Text.Json;
using Application.DTOs.Conversations;
using Domain.Constants;

namespace Application.Helper;

public static class ConversationAiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public static ConversationAiMessageContent ParseMessage(string json)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<ConversationAiMessageContent>(json, JsonOptions);
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.Text))
                throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

            return new ConversationAiMessageContent
            {
                Text = parsed.Text.Trim(),
                Suggestions = NormalizeStringList(parsed.Suggestions, 3),
                NewVocabulary = NormalizeVocabulary(parsed.NewVocabulary),
                GrammarPoints = NormalizeStringList(parsed.GrammarPoints),
            };
        }
        catch (JsonException)
        {
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
        catch (NotSupportedException)
        {
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }

    public static ConversationAiEvaluationContent ParseEvaluation(string json)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<ConversationAiEvaluationContent>(json, JsonOptions);
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.Feedback))
                throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

            return new ConversationAiEvaluationContent
            {
                Feedback = parsed.Feedback.Trim(),
                Score = Math.Clamp(parsed.Score, 0, 100),
            };
        }
        catch (JsonException)
        {
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
        catch (NotSupportedException)
        {
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }

    private static List<string> NormalizeStringList(IEnumerable<string>? items, int? maxCount = null)
    {
        var normalized = items?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList()
            ?? new List<string>();

        if (maxCount.HasValue)
            return normalized.Take(maxCount.Value).ToList();

        return normalized;
    }

    private static List<ConversationAiVocabularyContent> NormalizeVocabulary(IEnumerable<ConversationAiVocabularyContent>? items)
    {
        return items?
            .Where(x => !string.IsNullOrWhiteSpace(x.Word) && !string.IsNullOrWhiteSpace(x.Meaning))
            .Select(x => new ConversationAiVocabularyContent
            {
                Word = x.Word?.Trim() ?? string.Empty,
                Reading = x.Reading?.Trim() ?? string.Empty,
                Meaning = x.Meaning?.Trim() ?? string.Empty,
                Example = x.Example?.Trim() ?? string.Empty,
                JlptLevel = string.IsNullOrWhiteSpace(x.JlptLevel) ? "N5" : x.JlptLevel.Trim().ToUpperInvariant(),
            })
            .GroupBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList()
            ?? new List<ConversationAiVocabularyContent>();
    }
}
