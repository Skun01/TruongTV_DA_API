namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisQuestionInput
{
    public string QuestionId { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public string? Instruction { get; set; }
    public string? SelectedOptionText { get; set; }
    public string CorrectOptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}
