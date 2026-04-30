using Domain.Enums;

namespace Domain.Entities;

public class ExamSection : BaseEntity
{
    public string ExamId { get; set; } = string.Empty;
    public Exam Exam { get; set; } = null!;

    public SectionType SectionType { get; set; }
    public int OrderIndex { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }

    public ICollection<QuestionGroup> QuestionGroups { get; set; } = new List<QuestionGroup>();
    public ICollection<SessionSectionScore> SessionSectionScores { get; set; } = new List<SessionSectionScore>();
}
