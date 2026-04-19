namespace Application.DTOs.LearningAdmin;

public class DeckLearningModeAnalyticsResponse
{
    public string Mode { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public int CompletedSessionCount { get; set; }
    public int SubmissionCount { get; set; }
    public double AverageAccuracy { get; set; }
}
