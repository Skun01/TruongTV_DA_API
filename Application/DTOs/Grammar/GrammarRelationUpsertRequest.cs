namespace Application.DTOs.Grammar;

public class GrammarRelationUpsertRequest
{
    public string RelatedId { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
}
