namespace Domain.Entities;

public class DeckType
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Deck> Decks { get; set; } = new List<Deck>();
}
