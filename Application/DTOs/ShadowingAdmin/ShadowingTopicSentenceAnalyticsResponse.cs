namespace Application.DTOs.ShadowingAdmin;

public class ShadowingTopicSentenceAnalyticsResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Text { get; set; } = string.Empty;
    public int AttemptsCount { get; set; }
    public int DistinctUsersCount { get; set; }
    public double? AveragePronScore { get; set; }
    public DateTime? LatestAttemptAt { get; set; }
}
