namespace Application.DTOs.Sentences;

public class UpdateSentenceRequest
{
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Level { get; set; }
}
