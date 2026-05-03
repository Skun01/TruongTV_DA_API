namespace Application.DTOs.Dashboard.Learner;

public class ExamHistoryItem
{
    public string ExamSessionId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string ExamLevel { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? TotalScore { get; set; }
    public int MaxScore { get; set; }
    public bool? IsPassed { get; set; }
    public double Accuracy { get; set; }
}
