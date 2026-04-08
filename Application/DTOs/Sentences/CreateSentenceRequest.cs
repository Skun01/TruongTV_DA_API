namespace Application.DTOs.Sentences;

public class CreateSentenceRequest
{
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int? SpeakerId { get; set; }
    public string? Level { get; set; }
}
