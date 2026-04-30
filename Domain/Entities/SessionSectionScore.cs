namespace Domain.Entities;

public class SessionSectionScore : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public ExamSession Session { get; set; } = null!;

    public string SectionId { get; set; } = string.Empty;
    public ExamSection Section { get; set; } = null!;

    public int Score { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public bool IsPassed { get; set; }
}
