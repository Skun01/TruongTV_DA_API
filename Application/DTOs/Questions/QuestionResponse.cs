namespace Application.DTOs.Questions;

public class QuestionResponse
{
    public string Id { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageCaption { get; set; }
    public string? Explanation { get; set; }
    public int Score { get; set; }
    public int OrderIndex { get; set; }
    public List<QuestionOptionResponse> Options { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
