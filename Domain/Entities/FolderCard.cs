namespace Domain.Entities;

public class FolderCard
{
    public string DeckId { get; set; } = null!;
    public Deck Deck { get; set; } = null!;

    public string FolderId { get; set; } = null!;
    public DeckFolder Folder { get; set; } = null!;

    public string CardId { get; set; } = null!;
    public Card Card { get; set; } = null!;

    public int Position { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
