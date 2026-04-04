using Application.DTOs.Vocabulary;
using Application.Common;

namespace Application.IServices;

public interface IVocabularyDetailService
{
    Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string? currentUserId);
    Task<(List<VocabularyListItemResponse> Items, MetaData Meta)> SearchAsync(
        string? q,
        string? level,
        string? status,
        bool createdByMe,
        int page,
        int pageSize,
        string currentUserId);
    Task<VocabularyDetailResponse> CreateAsync(CreateVocabularyCardRequest request, string currentUserId);
    Task<VocabularyDetailResponse> UpdateAsync(string cardId, UpdateVocabularyCardRequest request, string currentUserId);
    Task<bool> SoftDeleteAsync(string cardId, string currentUserId);
}
