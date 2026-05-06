namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisExamInput
{
    public string SessionId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public bool IsPassed { get; set; }
    public int DurationMinutes { get; set; }
}
