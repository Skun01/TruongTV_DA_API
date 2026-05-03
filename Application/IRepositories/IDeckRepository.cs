using Domain.Entities;

namespace Application.IRepositories;

public interface IDeckRepository : IRepository<Deck>
{
    Task<(List<Deck> Items, int Total)> SearchPublicAsync(string? query, string? typeId, bool? officialOnly, int page, int pageSize, string? currentUserId);
    Task<(List<Deck> Items, int Total)> SearchOwnedByUserAsync(string userId, string? query, string? typeId, int page, int pageSize);
    Task<(List<Deck> Items, int Total)> SearchBookmarkedByUserAsync(string userId, string? query, string? typeId, int page, int pageSize);
    Task<(List<Deck> Items, int Total)> SearchAdminAsync(string? query, string? typeId, string? createdBy, string? status, string? visibility, bool? isOfficial, int page, int pageSize);
    Task<Deck?> GetDetailByIdAsync(string deckId, string? currentUserId);
    Task<Deck?> GetAdminDetailByIdAsync(string deckId);
    Task<Deck?> GetForkSourceByIdAsync(string deckId);
    Task<Deck?> GetManagedByIdAsync(string deckId);
    Task<Deck?> GetManagedDetailByIdAsync(string deckId);
    Task<Deck?> GetOwnedByIdAsync(string deckId, string userId);
    Task<Deck?> GetOwnedDetailByIdAsync(string deckId, string userId);
    Task<DeckFolder?> GetFolderByIdAsync(string folderId);
    Task<DeckFolder?> GetFolderWithCardsByIdAsync(string folderId);
    Task<DeckFolder?> GetOwnedFolderByIdAsync(string folderId, string userId);
    Task<DeckFolder?> GetOwnedFolderWithCardsByIdAsync(string folderId, string userId);
    Task<FolderCard?> GetFolderCardAsync(string folderId, string cardId);
    Task<bool> ExistsCardInDeckAsync(string deckId, string cardId);
    Task<bool> ExistsByTypeIdAsync(string typeId);
    Task<List<Deck>> GetReadableDecksContainingCardIdsAsync(string userId, List<string> cardIds);
    Task<List<Deck>> GetAdminDecksContainingCardIdsAsync(List<string> cardIds);
    Task<List<Deck>> GetAllReadableDecksWithFoldersAsync(string userId);
}
