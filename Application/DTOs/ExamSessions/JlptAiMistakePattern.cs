namespace Application.DTOs.ExamSessions;

public class JlptAiMistakePattern
{
    public string PatternId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public List<string> SectionTypes { get; set; } = new();
    public List<string> QuestionIds { get; set; } = new();
    public string Evidence { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
}
