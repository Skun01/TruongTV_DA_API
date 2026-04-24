namespace Application.DTOs.ShadowingAdmin;

public class ShadowingTopicAnalyticsResponse
{
    public string TopicId { get; set; } = string.Empty;
    public int AttemptsCount { get; set; }
    public int DistinctUsersCount { get; set; }
    public double? AveragePronScore { get; set; }
    public DateTime? LatestAttemptAt { get; set; }
}
