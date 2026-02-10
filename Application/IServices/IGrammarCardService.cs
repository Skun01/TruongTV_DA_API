using Application.DTOs.GrammarCard;

namespace Application.IServices;

public interface IGrammarCardService
{
    Task<bool> CreateGrammarCardAsync(CreateGrammarCardRequest request, string userId);
    Task<IEnumerable<GrammarCardDTO>> GetGrammarListByDeckIdAsync(string deckId, string userId);
}
