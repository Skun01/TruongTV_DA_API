namespace Application.DTOs.Decks;

public class DeckFolderCardItemResponse
{
    public string CardId { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime AddedAt { get; set; }
    public DeckCardSummaryResponse Card { get; set; } = new();
}
