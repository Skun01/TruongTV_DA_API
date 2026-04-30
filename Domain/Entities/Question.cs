namespace Domain.Entities;

public class Question : BaseEntity
{
    public string GroupId { get; set; } = string.Empty;
    public QuestionGroup Group { get; set; } = null!;

    public string QuestionText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageCaption { get; set; }
    public string? Explanation { get; set; }
    public int Score { get; set; } = 1;
    public int OrderIndex { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public ICollection<SessionAnswer> SessionAnswers { get; set; } = new List<SessionAnswer>();
}
