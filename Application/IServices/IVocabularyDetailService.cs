using Application.DTOs.Vocabulary;
using Application.Common;

namespace Application.IServices;

public interface IVocabularyDetailService
{
    Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string? currentUserId);
    Task<(List<VocabularyListItemResponse> Items, MetaData Meta)> SearchAsync(VocabularySearchQuery query, string currentUserId);
    Task<VocabularyDetailResponse> CreateAsync(CreateVocabularyCardRequest request, string currentUserId);
    Task<VocabularyDetailResponse> UpdateAsync(string cardId, UpdateVocabularyCardRequest request, string currentUserId);
    Task<bool> SoftDeleteAsync(string cardId, string currentUserId);
}
