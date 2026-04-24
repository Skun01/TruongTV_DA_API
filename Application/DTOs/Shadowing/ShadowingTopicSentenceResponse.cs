namespace Application.DTOs.Shadowing;

public class ShadowingTopicSentenceResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Level { get; set; }
    public string? Note { get; set; }
}
