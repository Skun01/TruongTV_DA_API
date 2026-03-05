using Application.DTOs.Deck;
using Application.DTOs.Learn;

namespace Application.IServices;

public interface ILearnService
{
    Task<LearnBatchDTO> GetLearnBatchAsync(string deckId, string userId);
    Task<bool> MarkCardLearnedAsync(MarkCardRequest request, string userId);
    Task<DailyProgressDTO> GetDailyProgressAsync(string userId);
    Task<DeckProgressDTO> GetDeckProgressAsync(string deckId, string userId);
}
