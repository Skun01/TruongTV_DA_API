namespace Application.DTOs.LearningAdmin;

public class DeckLearningAnalyticsResponse
{
    public string DeckId { get; set; } = string.Empty;
    public string DeckTitle { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public int CompletedSessionCount { get; set; }
    public int SubmissionCount { get; set; }
    public double AverageAccuracy { get; set; }
    public int TrackedCards { get; set; }
    public int MasteredCards { get; set; }
    public int DueCards { get; set; }
    public List<DeckLearningModeAnalyticsResponse> ModeBreakdown { get; set; } = new();
}
