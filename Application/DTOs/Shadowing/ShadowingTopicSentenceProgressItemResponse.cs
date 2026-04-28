namespace Application.DTOs.Shadowing;

public class ShadowingTopicSentenceProgressItemResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Level { get; set; }
    public int AttemptsCount { get; set; }
    public double? BestPronScore { get; set; }
    public double? LatestPronScore { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public bool HasAttempted { get; set; }
}
