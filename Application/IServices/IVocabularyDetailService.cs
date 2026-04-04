using Application.DTOs.Vocabulary;

namespace Application.IServices;

public interface IVocabularyDetailService
{
    Task<VocabularyDetailResponse> GetDetailAsync(string cardId, string currentUserId);
}
