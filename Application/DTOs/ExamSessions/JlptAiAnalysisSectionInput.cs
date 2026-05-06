namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisSectionInput
{
    public string SectionId { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public bool IsPassed { get; set; }
}
