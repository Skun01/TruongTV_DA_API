using Application.DTOs.Shadowing;
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
            Level = topic.Level?.ToString(),
            Visibility = topic.Visibility.ToString(),
            Status = topic.Status.ToString(),
            IsOfficial = topic.IsOfficial,
            SentencesCount = topic.SentencesCount,
            IsOwner = !string.IsNullOrWhiteSpace(currentUserId) && topic.CreatedBy == currentUserId,
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
            Level = topic.Level?.ToString(),
            Visibility = topic.Visibility.ToString(),
            Status = topic.Status.ToString(),
            IsOfficial = topic.IsOfficial,
            SentencesCount = topic.SentencesCount,
            IsOwner = !string.IsNullOrWhiteSpace(currentUserId) && topic.CreatedBy == currentUserId,
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
}
