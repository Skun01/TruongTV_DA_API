namespace Domain.Entities;

// Many-to-many join entity
public class CardSentence
{
    public string CardId { get; set; } = string.Empty;
    public Card Card { get; set; } = null!;

    public string SentenceId { get; set; } = string.Empty;
    public Sentence Sentence { get; set; } = null!;

    public int Position { get; set; }
    public string? BlankWord { get; set; }
    public string? Hint { get; set; }
    public List<string> AnswerList { get; set; } = new();
}
