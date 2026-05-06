namespace Application.DTOs.ExamSessions;

public class JlptAiSectionAnalysis
{
    public string SectionType { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public bool IsPassed { get; set; }
    public string PerformanceBand { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> RecommendedFocus { get; set; } = new();
}
