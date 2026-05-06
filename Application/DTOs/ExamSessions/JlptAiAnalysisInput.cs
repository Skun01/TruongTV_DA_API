namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisInput
{
    public JlptAiAnalysisExamInput Exam { get; set; } = new();
    public List<JlptAiAnalysisSectionInput> Sections { get; set; } = new();
    public List<JlptAiAnalysisQuestionInput> Questions { get; set; } = new();
}
