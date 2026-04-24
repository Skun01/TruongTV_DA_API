namespace Domain.Entities;

public class ShadowingTopicSentence
{
    public string TopicId { get; set; } = string.Empty;
    public ShadowingTopic Topic { get; set; } = null!;

    public string SentenceId { get; set; } = string.Empty;
    public Sentence Sentence { get; set; } = null!;

    public int Position { get; set; }
    public string? Note { get; set; }
}
