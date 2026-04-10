using Domain.Enums;

namespace Domain.Entities;

public class GrammarRelation
{
    public string GrammarId { get; set; } = string.Empty;
    public Card Grammar { get; set; } = null!;

    public string RelatedId { get; set; } = string.Empty;
    public Card Related { get; set; } = null!;

    public GrammarRelationType RelationType { get; set; }
}
