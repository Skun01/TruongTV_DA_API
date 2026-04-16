using Domain.Enums;

namespace Domain.Entities;

public class Deck : BaseEntity
{
    public string CreatedBy { get; set; } = null!;
    public User Creator { get; set; } = null!;

    public string? ForkedFromId { get; set; }
    public Deck? ForkedFrom { get; set; }

    public string? TypeId { get; set; }
    public DeckType? Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public DeckVisibility Visibility { get; set; } = DeckVisibility.Public;
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public bool IsOfficial { get; set; }
    public int CardsCount { get; set; }
    public int FoldersCount { get; set; }

    public ICollection<Deck> Forks { get; set; } = new List<Deck>();
    public ICollection<DeckFolder> Folders { get; set; } = new List<DeckFolder>();
    public ICollection<FolderCard> FolderCards { get; set; } = new List<FolderCard>();
    public ICollection<DeckBookmark> Bookmarks { get; set; } = new List<DeckBookmark>();
}
