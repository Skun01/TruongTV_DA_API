namespace Application.DTOs.ExamSessions;

/// <summary>
/// Option cho học viên — ẩn IsCorrect khi đang làm bài
/// </summary>
public class SessionOptionResponse
{
    public string OptionId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string OptionType { get; set; } = string.Empty;
}
