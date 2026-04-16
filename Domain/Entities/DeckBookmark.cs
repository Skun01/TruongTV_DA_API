namespace Domain.Entities;

public class DeckBookmark
{
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public string DeckId { get; set; } = null!;
    public Deck Deck { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
