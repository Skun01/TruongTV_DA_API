namespace Application.DTOs.Vocabulary;

public class VocabularySentenceResponse
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Level { get; set; }
}
