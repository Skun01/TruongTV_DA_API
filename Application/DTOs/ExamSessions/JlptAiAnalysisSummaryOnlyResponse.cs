namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisSummaryOnlyResponse
{
    public string AnalysisId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public JlptAiAnalysisSummary Summary { get; set; } = new();
    public List<JlptAiSectionAnalysis> SectionAnalyses { get; set; } = new();
    public List<JlptAiNextAction> NextActions { get; set; } = new();
}