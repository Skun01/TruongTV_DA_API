namespace Application.DTOs.Dashboard.Learner;

public class RecentSessionSummary
{
    public string Id { get; set; } = string.Empty;
    public string? DeckTitle { get; set; }
    public string Mode { get; set; } = string.Empty;
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public double Accuracy { get; set; }
    public DateTime? CompletedAt { get; set; }
}
