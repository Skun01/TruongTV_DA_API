using Domain.Entities;

namespace Application.IRepositories;

public interface IDeckQueueRepository : IRepository<DeckQueue>
{
    Task<IEnumerable<DeckQueue>> GetByUserIdAsync(string userId);
    Task<DeckQueue?> GetByUserAndDeckAsync(string userId, string deckId);
    Task<bool> IsExist(string userId, string deckId);
}
