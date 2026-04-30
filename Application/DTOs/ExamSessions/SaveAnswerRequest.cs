namespace Application.DTOs.ExamSessions;

public class SaveAnswerRequest
{
    public string QuestionId { get; set; } = string.Empty;
    public string? SelectedOptionId { get; set; }
}
