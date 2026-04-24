namespace Application.DTOs.Shadowing;

public class ShadowingTopicDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsOfficial { get; set; }
    public int SentencesCount { get; set; }
    public bool IsOwner { get; set; }
    public List<ShadowingTopicSentenceResponse> Sentences { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
