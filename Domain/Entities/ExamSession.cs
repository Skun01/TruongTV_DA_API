using Domain.Enums;

namespace Domain.Entities;

public class ExamSession : BaseEntity
{
    public string ExamId { get; set; } = string.Empty;
    public Exam Exam { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ExamSessionStatus Status { get; set; } = ExamSessionStatus.InProgress;
    public int? TotalScore { get; set; }
    public bool? IsPassed { get; set; }

    public ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
    public ICollection<SessionSectionScore> SectionScores { get; set; } = new List<SessionSectionScore>();
}
