using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface ICardProgressRepository : IRepository<CardProgress>
{
    Task<CardProgress?> GetByUserAndCardAsync(string userId, string cardId, DeckType cardType);
    Task<List<CardProgress>> GetByUserAndCardIdsAsync(string userId, IEnumerable<string> cardIds, DeckType cardType);
    Task<List<CardProgress>> GetDueForReviewAsync(string userId, IEnumerable<string> cardIds, DeckType cardType);
    Task<int> GetTodayLearnedCountAsync(string userId);
    Task<int> GetCountByStatusAsync(string userId, IEnumerable<string> cardIds, DeckType cardType, CardStatus status);
    Task<int> GetDueCountAsync(string userId, IEnumerable<string> cardIds, DeckType cardType);
    Task<(int correct, int total)> GetAccuracyAsync(string userId, IEnumerable<string> cardIds, DeckType cardType);
}
