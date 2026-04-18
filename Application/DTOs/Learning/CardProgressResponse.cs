namespace Application.DTOs.Learning;

public class CardProgressResponse
{
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string SrsLevel { get; set; } = string.Empty;
    public DateTime NextReviewAt { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public int ConsecutiveCorrect { get; set; }
    public bool IsMastered { get; set; }
    public string? LastSentenceId { get; set; }
}
