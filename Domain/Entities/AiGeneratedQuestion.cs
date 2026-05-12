using Domain.Enums;

namespace Domain.Entities;

public class AiGeneratedQuestion : BaseEntity
{
    public JlptLevel Level { get; set; }
    public SectionType SectionType { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? QuestionGroupId { get; set; }
    public QuestionGroup? QuestionGroup { get; set; }
    public string GeneratedData { get; set; } = string.Empty;
    public AiQuestionStatus Status { get; set; } = AiQuestionStatus.Pending;

    public string? ReviewedBy { get; set; }
    public User? Reviewer { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public string? QuestionId { get; set; }
    public Question? Question { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public User Creator { get; set; } = null!;
}
