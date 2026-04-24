namespace Application.DTOs.ShadowingAdmin;

public class ReorderShadowingTopicSentencesRequest
{
    public List<ReorderShadowingTopicSentenceItemRequest> Items { get; set; } = new();
}
