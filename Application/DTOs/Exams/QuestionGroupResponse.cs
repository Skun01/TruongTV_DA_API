using Application.DTOs.Questions;

namespace Application.DTOs.Exams;

public class QuestionGroupResponse
{
    public string Id { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public string? AudioUrl { get; set; }
    public string? AudioScript { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? MondaiType { get; set; }
    public List<QuestionResponse> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
