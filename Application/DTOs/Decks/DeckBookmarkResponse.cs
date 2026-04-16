namespace Application.DTOs.Decks;

public class DeckBookmarkResponse
{
    public string DeckId { get; set; } = string.Empty;
    public bool Bookmarked { get; set; }
    public DateTime? SavedAt { get; set; }
}
