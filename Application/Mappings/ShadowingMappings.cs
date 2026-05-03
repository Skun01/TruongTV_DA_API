using System.Text.Json;
using Application.DTOs.Shadowing;
using Application.DTOs.ShadowingAdmin;
using Domain.Entities;

namespace Application.Mappings;

public static class ShadowingMappings
{
    public static ShadowingTopicListItemResponse ToTopicListItemResponse(this ShadowingTopic topic, string? currentUserId)
    {
        return new ShadowingTopicListItemResponse
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            CoverImageUrl = topic.CoverImageUrl,
            Level = topic.Level?.ToString(),
            Visibility = topic.Visibility.ToString(),
            Status = topic.Status.ToString(),
            IsOfficial = topic.IsOfficial,
            SentencesCount = topic.SentencesCount,
            IsOwner = !string.IsNullOrWhiteSpace(currentUserId) && topic.CreatedBy == currentUserId,
            CreatorId = topic.CreatedBy,
            CreatorName = topic.Creator.Username,
            CreatedAt = topic.CreatedAt,
            UpdatedAt = topic.UpdatedAt,
        };
    }

    public static ShadowingTopicDetailResponse ToTopicDetailResponse(this ShadowingTopic topic, string? currentUserId)
    {
        return new ShadowingTopicDetailResponse
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            CoverImageUrl = topic.CoverImageUrl,
            Level = topic.Level?.ToString(),
            Visibility = topic.Visibility.ToString(),
            Status = topic.Status.ToString(),
            IsOfficial = topic.IsOfficial,
            SentencesCount = topic.SentencesCount,
            IsOwner = !string.IsNullOrWhiteSpace(currentUserId) && topic.CreatedBy == currentUserId,
            CreatorId = topic.CreatedBy,
            CreatorName = topic.Creator.Username,
            Sentences = topic.TopicSentences
                .Where(x => x.Sentence != null)
                .OrderBy(x => x.Position)
                .Select(x => x.ToTopicSentenceResponse())
                .ToList(),
            CreatedAt = topic.CreatedAt,
            UpdatedAt = topic.UpdatedAt,
        };
    }

    public static ShadowingTopicSentenceResponse ToTopicSentenceResponse(this ShadowingTopicSentence topicSentence)
    {
        return new ShadowingTopicSentenceResponse
        {
            SentenceId = topicSentence.SentenceId,
            Position = topicSentence.Position,
            Text = topicSentence.Sentence.Text,
            Meaning = topicSentence.Sentence.Meaning,
            AudioUrl = topicSentence.Sentence.AudioUrl,
            Level = topicSentence.Sentence.Level?.ToString(),
            Note = topicSentence.Note,
        };
    }

    public static ShadowingAttemptResponse ToAttemptResponse(this ShadowingAttempt attempt)
    {
        return new ShadowingAttemptResponse
        {
            AttemptId = attempt.Id,
            TopicId = attempt.TopicId,
            TopicTitle = attempt.Topic.Title,
            SentenceId = attempt.SentenceId,
            SentenceText = attempt.Sentence.Text,
            AudioAssetId = attempt.AudioAssetId,
            AudioUrl = attempt.AudioAsset.FileUrl,
            Locale = attempt.Locale,
            RecognizedText = attempt.RecognizedText,
            PronScore = attempt.PronScore,
            AccuracyScore = attempt.AccuracyScore,
            FluencyScore = attempt.FluencyScore,
            CompletenessScore = attempt.CompletenessScore,
            ProsodyScore = attempt.ProsodyScore,
            ErrorTypes = string.IsNullOrWhiteSpace(attempt.ErrorTypes)
                ? []
                : attempt.ErrorTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            WordAssessments = ParseWordAssessments(attempt.RawResultJson),
            DurationMs = attempt.DurationMs,
            CreatedAt = attempt.CreatedAt,
        };
    }

    public static ShadowingAttemptHistoryItemResponse ToAttemptHistoryItemResponse(this ShadowingAttempt attempt)
    {
        return new ShadowingAttemptHistoryItemResponse
        {
            AttemptId = attempt.Id,
            TopicId = attempt.TopicId,
            TopicTitle = attempt.Topic.Title,
            SentenceId = attempt.SentenceId,
            SentenceText = attempt.Sentence.Text,
            Locale = attempt.Locale,
            PronScore = attempt.PronScore,
            AccuracyScore = attempt.AccuracyScore,
            FluencyScore = attempt.FluencyScore,
            CompletenessScore = attempt.CompletenessScore,
            ProsodyScore = attempt.ProsodyScore,
            CreatedAt = attempt.CreatedAt,
        };
    }

    public static AdminShadowingAvailableSentenceResponse ToAvailableSentenceResponse(this Sentence sentence, ShadowingTopicSentence? attachedLink)
    {
        return new AdminShadowingAvailableSentenceResponse
        {
            SentenceId = sentence.Id,
            Text = sentence.Text,
            Meaning = sentence.Meaning,
            AudioUrl = sentence.AudioUrl,
            Level = sentence.Level?.ToString(),
            IsAttached = attachedLink != null,
            AttachedPosition = attachedLink?.Position,
            AttachedNote = attachedLink?.Note,
        };
    }

    public static ShadowingTopicSentenceAnalyticsResponse ToSentenceAnalyticsResponse(
        this ShadowingTopicSentence topicSentence,
        int attemptsCount,
        int distinctUsersCount,
        double? averagePronScore,
        DateTime? latestAttemptAt)
    {
        return new ShadowingTopicSentenceAnalyticsResponse
        {
            SentenceId = topicSentence.SentenceId,
            Position = topicSentence.Position,
            Text = topicSentence.Sentence.Text,
            AttemptsCount = attemptsCount,
            DistinctUsersCount = distinctUsersCount,
            AveragePronScore = averagePronScore,
            LatestAttemptAt = latestAttemptAt,
        };
    }

    private static List<ShadowingAttemptWordAssessmentResponse> ParseWordAssessments(string? rawResultJson)
    {
        if (string.IsNullOrWhiteSpace(rawResultJson))
            return [];

        try
        {
            using var document = JsonDocument.Parse(rawResultJson);
            if (!document.RootElement.TryGetProperty("NBest", out var nBestElement)
                || nBestElement.ValueKind != JsonValueKind.Array
                || nBestElement.GetArrayLength() == 0)
            {
                return [];
            }

            var firstBest = nBestElement[0];
            if (!firstBest.TryGetProperty("Words", out var wordsElement)
                || wordsElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var results = new List<ShadowingAttemptWordAssessmentResponse>();

            foreach (var wordElement in wordsElement.EnumerateArray())
            {
                var word = TryGetString(wordElement, "Word");
                var displayWord = TryGetString(wordElement, "DisplayWord") ?? word;
                double? accuracyScore = null;
                var errorType = TryGetString(wordElement, "ErrorType");

                if (wordElement.TryGetProperty("PronunciationAssessment", out var assessmentElement))
                {
                    accuracyScore = TryGetDouble(assessmentElement, "AccuracyScore");
                    errorType ??= TryGetString(assessmentElement, "ErrorType");
                }

                accuracyScore ??= TryGetDouble(wordElement, "AccuracyScore");

                if (string.IsNullOrWhiteSpace(word) && string.IsNullOrWhiteSpace(displayWord))
                    continue;

                results.Add(new ShadowingAttemptWordAssessmentResponse
                {
                    Word = word ?? string.Empty,
                    DisplayWord = displayWord,
                    AccuracyScore = accuracyScore,
                    ErrorType = errorType,
                });
            }

            return results;
        }
        catch
        {
            return [];
        }
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static double? TryGetDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
               && value.ValueKind == JsonValueKind.Number
               && value.TryGetDouble(out var score)
            ? score
            : null;
    }
}
