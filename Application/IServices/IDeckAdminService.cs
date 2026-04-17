using Application.Common;
using Application.DTOs.Decks;

namespace Application.IServices;

public interface IDeckAdminService
{
    Task<(List<AdminDeckListItemResponse> Items, MetaData Meta)> SearchAsync(AdminDeckListQuery query);
    Task<AdminDeckDetailResponse> GetDetailAsync(string deckId);
    Task<AdminDeckDetailResponse> CreateAsync(CreateAdminDeckRequest request, string currentUserId);
    Task<AdminDeckDetailResponse> UpdateAsync(string deckId, UpdateAdminDeckRequest request);
    Task<bool> DeleteAsync(string deckId);
    Task<AdminDeckDetailResponse> PublishAsync(string deckId);
    Task<AdminDeckDetailResponse> ArchiveAsync(string deckId);
    Task<AdminDeckDetailResponse> UnpublishAsync(string deckId);
    Task<DeckFolderResponse> CreateFolderAsync(string deckId, CreateDeckFolderRequest request);
    Task<DeckFolderResponse> UpdateFolderAsync(string folderId, UpdateDeckFolderRequest request);
    Task<bool> DeleteFolderAsync(string folderId);
    Task<DeckFolderResponse> AddCardToFolderAsync(string folderId, AddCardToFolderRequest request);
    Task<bool> RemoveCardFromFolderAsync(string folderId, string cardId);
    Task<List<DeckFolderResponse>> ReorderDeckFoldersAsync(string deckId, ReorderDeckFoldersRequest request);
    Task<List<DeckFolderCardItemResponse>> ReorderFolderCardsAsync(string folderId, ReorderFolderCardsRequest request);
}
