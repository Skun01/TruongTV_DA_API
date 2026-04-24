using Domain.Enums;

namespace Domain.Entities;

public class ShadowingTopic : BaseEntity
{
    public string CreatedBy { get; set; } = string.Empty;
    public User Creator { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JlptLevel? Level { get; set; }
    public DeckVisibility Visibility { get; set; } = DeckVisibility.Public;
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public bool IsOfficial { get; set; }
    public int SentencesCount { get; set; }

    public ICollection<ShadowingTopicSentence> TopicSentences { get; set; } = new List<ShadowingTopicSentence>();
    public ICollection<ShadowingAttempt> Attempts { get; set; } = new List<ShadowingAttempt>();
}
