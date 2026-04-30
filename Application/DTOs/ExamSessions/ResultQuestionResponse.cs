namespace Application.DTOs.ExamSessions;

/// <summary>
/// Chi tiết từng câu hỏi trong kết quả — hiện đáp án đúng và giải thích
/// </summary>
public class ResultQuestionResponse
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
    public string SectionType { get; set; } = string.Empty;
    public string? SelectedOptionId { get; set; }
    public string? CorrectOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public List<SessionOptionResponse> Options { get; set; } = new();
}
