namespace Application.DTOs.Shadowing;

public class ShadowingSentenceProgressResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public int AttemptsCount { get; set; }
    public double? BestPronScore { get; set; }
    public double? LatestPronScore { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
