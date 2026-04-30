using Domain.Enums;

namespace Domain.Entities;

public class QuestionGroup : BaseEntity
{
    public string SectionId { get; set; } = string.Empty;
    public ExamSection Section { get; set; } = null!;

    public string? PassageText { get; set; }
    public string? AudioUrl { get; set; }
    public string? AudioScript { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public ChoukaiMondaiType? MondaiType { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
