namespace Application.DTOs.Questions;

public class QuestionOptionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string OptionType { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
