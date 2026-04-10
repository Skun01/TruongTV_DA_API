namespace Application.DTOs.Grammar;

public class GrammarStructureResponse
{
    public string Pattern { get; set; } = string.Empty;
    public Dictionary<string, string>? Annotations { get; set; }
}
