namespace Application.DTOs.ShadowingAdmin;

public class ReorderShadowingTopicSentenceItemRequest
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
}
