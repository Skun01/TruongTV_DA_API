using Domain.Enums;

namespace Domain.Entities;

public class ExtractedVocabulary : BaseEntity
{
    public string MessageId { get; set; } = string.Empty;
    public ConversationMessage Message { get; set; } = null!;

    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
    public JlptLevel JlptLevel { get; set; }
}
