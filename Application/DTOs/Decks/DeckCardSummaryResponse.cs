namespace Application.DTOs.Decks;

public class DeckCardSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string? Level { get; set; }
}
