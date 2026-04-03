namespace Domain.Entities;

// Many-to-many join entity
public class CardSentence
{
    public string CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public string SentenceId { get; set; }
    public Sentence Sentence { get; set; } = null!;
}
