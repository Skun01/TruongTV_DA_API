namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisContent
{
    public JlptAiAnalysisSummary Summary { get; set; } = new();
    public List<JlptAiSectionAnalysis> SectionAnalyses { get; set; } = new();
    public List<JlptAiMistakePattern> MistakePatterns { get; set; } = new();
    public List<JlptAiQuestionInsight> QuestionInsights { get; set; } = new();
    public List<JlptAiRecommendation> Recommendations { get; set; } = new();
    public List<JlptAiNextAction> NextActions { get; set; } = new();
}
