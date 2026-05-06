namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisResponse
{
    public string AnalysisId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Model { get; set; } = string.Empty;
    public string PromptVersion { get; set; } = string.Empty;
    public JlptAiAnalysisSummary Summary { get; set; } = new();
    public List<JlptAiSectionAnalysis> SectionAnalyses { get; set; } = new();
    public List<JlptAiMistakePattern> MistakePatterns { get; set; } = new();
    public List<JlptAiQuestionInsight> QuestionInsights { get; set; } = new();
    public List<JlptAiRecommendation> Recommendations { get; set; } = new();
    public List<JlptAiNextAction> NextActions { get; set; } = new();
}
