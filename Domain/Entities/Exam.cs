using Domain.Enums;

namespace Domain.Entities;

public class Exam : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public JlptLevel Level { get; set; }
    public int TotalDurationMinutes { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;

    public string CreatedBy { get; set; } = string.Empty;
    public User Creator { get; set; } = null!;

    public ICollection<ExamSection> Sections { get; set; } = new List<ExamSection>();
    public ICollection<ExamSession> ExamSessions { get; set; } = new List<ExamSession>();
}
