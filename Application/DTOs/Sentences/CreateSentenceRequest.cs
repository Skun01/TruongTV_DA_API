namespace Application.DTOs.Sentences;

public class CreateSentenceRequest
{
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Level { get; set; }
}