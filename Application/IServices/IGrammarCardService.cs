using Application.DTOs.GrammarCard;

namespace Application.IServices;

public interface IGrammarCardService
{
    Task<bool> CreateGrammarCardAsync(CreateGrammarCardRequest request, string userId);
    Task<IEnumerable<GrammarCardDTO>> GetGrammarListByDeckIdAsync(string deckId, string userId);
    Task<GrammarCardDTO> GetCardByIdAsync(string id, string userId);
    Task<bool> UpdateCardByIdAsync(UpdateGrammarCardRequest request, string cardId, string userId);
    Task<bool> DeleteByIdAsync(string cardId, string userId);
}
