using Application.DTOs.VocabularyCard;

namespace Application.IServices;

public interface IVocabularyCardService
{
    Task<bool> CreateVocabularyCardAsync(CreateVocabularyRequest request, string userId);
    Task<IEnumerable<VocabularyCardDTO>> GetVocabularyListByDeckIdAsync(string deckId);
    Task<VocabularyCardDTO> GetCardByIdAsync(string id, string userId);
    Task<bool> UpdateCardByIdAsync(UpdateVocabularyCardRequest request, string cardId, string userId);
    Task<bool> DeleteByIdAsync(string id, string userId);
}
