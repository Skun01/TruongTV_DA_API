namespace Application.DTOs.Shadowing;

public class ShadowingTopicResumeResponse
{
    public string TopicId { get; set; } = string.Empty;
    public string? RecommendedSentenceId { get; set; }
    public string? LastAttemptSentenceId { get; set; }
    public int AttemptedSentencesCount { get; set; }
    public int RemainingSentencesCount { get; set; }
    public double? LatestPronScore { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
