using Domain.Enums;

namespace Domain.Entities;

public class Card : BaseEntity
{
    public CardType CardType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public JlptLevel? Level { get; set; }
    
    // Will be explicitly mapped to text[] in Postgres
    public List<string> Tags { get; set; } = new();
    
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    
    public string CreatedBy { get; set; }
    public User Creator { get; set; } = null!;
    
    // Navigation
    public VocabularyDetail? VocabularyDetail { get; set; }
    public ICollection<CardSentence> CardSentences { get; set; } = new List<CardSentence>();
    public ICollection<UserCardNote> UserCardNotes { get; set; } = new List<UserCardNote>();
}
