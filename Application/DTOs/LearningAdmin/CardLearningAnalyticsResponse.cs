namespace Application.DTOs.LearningAdmin;

public class CardLearningAnalyticsResponse
{
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int IncludedSessionCount { get; set; }
    public int IncludedCompletedSessionCount { get; set; }
    public int TrackedUsers { get; set; }
    public int MasteredUsers { get; set; }
    public int DueUsers { get; set; }
    public double AverageSrsLevel { get; set; }
    public double AverageConsecutiveCorrect { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public List<CardLearningSrsDistributionResponse> SrsDistribution { get; set; } = new();
    public List<CardLearningDeckUsageResponse> Decks { get; set; } = new();
}
