namespace Domain.Entities;

public class DeckFolder : BaseEntity
{
    public string DeckId { get; set; } = null!;
    public Deck Deck { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Position { get; set; }
    public int CardsCount { get; set; }

    public ICollection<FolderCard> FolderCards { get; set; } = new List<FolderCard>();
}
