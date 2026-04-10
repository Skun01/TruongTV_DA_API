namespace Application.DTOs.Grammar;

public class GrammarSentenceUpsertRequest
{
    public string? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public int? SpeakerId { get; set; }
    public string? Level { get; set; }
}
