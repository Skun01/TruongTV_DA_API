namespace Application.DTOs.Questions;

public class CreateQuestionOptionRequest
{
    public string Label { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string OptionType { get; set; } = "Text";
    public bool IsCorrect { get; set; }
}
