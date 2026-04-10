namespace Domain.ValueObjects;

public class GrammarStructureItem
{
    public string Pattern { get; set; } = string.Empty;
    public Dictionary<string, string>? Annotations { get; set; }
}
