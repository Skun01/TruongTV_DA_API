namespace Application.DTOs.ShadowingAdmin;

public class BulkAttachShadowingTopicSentencesRequest
{
    public List<AttachShadowingTopicSentenceRequest> Items { get; set; } = new();
}
