namespace Domain.Entities;

public class VocabularyCard : BaseEntity
{
    public string Term { set; get; } = string.Empty;
    public string Meaning { set; get; } = string.Empty;
    public string DeckId { set; get; } = string.Empty;

    public virtual Deck? Deck { set; get; }
    public virtual ICollection<ExampleSentence> ExampleSentences { set; get; } = [];
}
