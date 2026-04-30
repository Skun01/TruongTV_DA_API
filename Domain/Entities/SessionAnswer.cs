namespace Domain.Entities;

public class SessionAnswer : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public ExamSession Session { get; set; } = null!;

    public string QuestionId { get; set; } = string.Empty;
    public Question Question { get; set; } = null!;

    public string? SelectedOptionId { get; set; }
    public QuestionOption? SelectedOption { get; set; }

    public DateTime AnsweredAt { get; set; }
}
