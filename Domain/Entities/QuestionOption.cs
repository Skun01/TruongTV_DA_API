using Domain.Enums;

namespace Domain.Entities;

public class QuestionOption : BaseEntity
{
    public string QuestionId { get; set; } = string.Empty;
    public Question Question { get; set; } = null!;

    public OptionLabel Label { get; set; }
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public OptionType OptionType { get; set; } = OptionType.Text;
    public bool IsCorrect { get; set; }
}
