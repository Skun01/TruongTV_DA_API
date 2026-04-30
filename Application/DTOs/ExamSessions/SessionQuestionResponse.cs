namespace Application.DTOs.ExamSessions;

/// <summary>
/// Câu hỏi cho học viên — ẩn IsCorrect
/// </summary>
public class SessionQuestionResponse
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageCaption { get; set; }
    public int OrderIndex { get; set; }
    public List<SessionOptionResponse> Options { get; set; } = new();
    public string? SelectedOptionId { get; set; }
}
