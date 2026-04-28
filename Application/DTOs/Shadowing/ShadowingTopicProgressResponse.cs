namespace Application.DTOs.Shadowing;

public class ShadowingTopicProgressResponse
{
    public string TopicId { get; set; } = string.Empty;
    public int SentencesCount { get; set; }
    public int AttemptedSentencesCount { get; set; }
    public int CompletedSentencesCount { get; set; }
    public double? BestPronScore { get; set; }
    public double? LatestPronScore { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
