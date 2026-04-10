namespace Application.DTOs.Grammar;

public class ImportGrammarRequest
{
    public List<ImportGrammarItemRequest> Items { get; set; } = new();
}
