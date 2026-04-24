namespace Application.DTOs.ShadowingAdmin;

public class AttachShadowingTopicSentenceRequest
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string? Note { get; set; }
}
