namespace Application.DTOs.Decks;

public class AdminDeckDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsOfficial { get; set; }
    public int CardsCount { get; set; }
    public int FoldersCount { get; set; }
    public DeckTypeSummaryResponse Type { get; set; } = new();
    public DeckCreatorResponse CreatedBy { get; set; } = new();
    public string? ForkedFromId { get; set; }
    public int BookmarkCount { get; set; }
    public List<DeckFolderResponse> Folders { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
