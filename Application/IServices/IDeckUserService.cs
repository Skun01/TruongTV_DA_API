using Application.Common;
using Application.DTOs.Decks;

namespace Application.IServices;

public interface IDeckUserService
{
    Task<(List<DeckListItemResponse> Items, MetaData Meta)> SearchPublicAsync(DeckListQuery query, string? currentUserId);
    Task<DeckDetailResponse> GetDetailAsync(string deckId, string? currentUserId);
    Task<DeckBookmarkResponse> ToggleBookmarkAsync(string deckId, string userId, ToggleDeckBookmarkRequest request);
    Task<(List<DeckListItemResponse> Items, MetaData Meta)> GetBookmarkedAsync(BookmarkedDeckListQuery query, string userId);
    Task<DeckDetailResponse> ForkAsync(string deckId, string userId);
    Task<(List<DeckListItemResponse> Items, MetaData Meta)> GetMyDecksAsync(MyDeckListQuery query, string userId);
    Task<DeckDetailResponse> CreateMyDeckAsync(CreateMyDeckRequest request, string userId);
    Task<DeckDetailResponse> UpdateMyDeckAsync(string deckId, UpdateMyDeckRequest request, string userId);
    Task<bool> DeleteMyDeckAsync(string deckId, string userId);
    Task<DeckFolderResponse> CreateFolderAsync(string deckId, CreateDeckFolderRequest request, string userId);
    Task<DeckFolderResponse> UpdateFolderAsync(string folderId, UpdateDeckFolderRequest request, string userId);
    Task<bool> DeleteFolderAsync(string folderId, string userId);
    Task<DeckFolderResponse> AddCardToFolderAsync(string folderId, AddCardToFolderRequest request, string userId);
    Task<bool> RemoveCardFromFolderAsync(string folderId, string cardId, string userId);
    Task<List<DeckFolderResponse>> ReorderDeckFoldersAsync(string deckId, ReorderDeckFoldersRequest request, string userId);
    Task<List<DeckFolderCardItemResponse>> ReorderFolderCardsAsync(string folderId, ReorderFolderCardsRequest request, string userId);
}
