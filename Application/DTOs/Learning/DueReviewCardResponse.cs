namespace Application.DTOs.Learning;

public class DueReviewCardResponse
{
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? DeckId { get; set; }
    public string? DeckTitle { get; set; }
    public string SrsLevel { get; set; } = string.Empty;
    public DateTime NextReviewAt { get; set; }
    public bool IsMastered { get; set; }
}
