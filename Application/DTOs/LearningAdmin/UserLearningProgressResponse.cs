namespace Application.DTOs.LearningAdmin;

public class UserLearningProgressResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalTrackedCards { get; set; }
    public int MasteredCards { get; set; }
    public int DueCards { get; set; }
    public double AverageSrsLevel { get; set; }
    public double AverageConsecutiveCorrect { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public int RecentSessionCount { get; set; }
    public List<UserLearningSrsDistributionResponse> SrsDistribution { get; set; } = new();
    public List<UserLearningDeckProgressResponse> Decks { get; set; } = new();
}
