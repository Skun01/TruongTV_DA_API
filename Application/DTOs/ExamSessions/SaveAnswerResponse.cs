namespace Application.DTOs.ExamSessions;

public class SaveAnswerResponse
{
    public string QuestionId { get; set; } = string.Empty;
    public string? SelectedOptionId { get; set; }
    public DateTime SavedAt { get; set; }
}
