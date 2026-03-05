namespace Application.DTOs.Review;

public class ReviewItemDTO
{
    public string CardProgressId { set; get; } = string.Empty;
    public string? ExampleSentenceId { set; get; }
    public string ReviewType { set; get; } = string.Empty; // "Cloze" or "Flashcard"
    public string? ClozeSentence { set; get; }
    public string? Hint { set; get; }
    public string CardTerm { set; get; } = string.Empty;
    public string CardMeaning { set; get; } = string.Empty;
    public int SrsLevel { set; get; }
}
