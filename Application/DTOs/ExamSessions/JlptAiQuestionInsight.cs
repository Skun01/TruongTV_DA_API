namespace Application.DTOs.ExamSessions;

public class JlptAiQuestionInsight
{
    public string QuestionId { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? SelectedOptionId { get; set; }
    public string CorrectOptionId { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> ReviewTags { get; set; } = new();
}
