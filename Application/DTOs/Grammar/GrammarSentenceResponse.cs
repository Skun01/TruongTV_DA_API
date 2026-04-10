namespace Application.DTOs.Grammar;

public class GrammarSentenceResponse
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Level { get; set; }
}
