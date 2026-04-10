namespace Application.DTOs.Grammar;

public class CreateGrammarCardRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Status { get; set; }

    public List<GrammarStructureRequest> Structures { get; set; } = new();
    public string? Explanation { get; set; }
    public string? Caution { get; set; }
    public string? Register { get; set; }
    public List<string> AlternateForms { get; set; } = new();
    public List<GrammarRelationUpsertRequest> Relations { get; set; } = new();
    public List<GrammarResourceUpsertRequest> Resources { get; set; } = new();
}
