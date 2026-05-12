namespace Application.DTOs.AiQuestions;

public class AiGeneratedQuestionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? QuestionGroupId { get; set; }
    public string GeneratedData { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReviewedBy { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? QuestionId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
