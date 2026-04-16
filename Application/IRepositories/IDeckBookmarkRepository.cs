using Domain.Entities;

namespace Application.IRepositories;

public interface IDeckBookmarkRepository : IRepository<DeckBookmark>
{
    Task<DeckBookmark?> GetByUserAndDeckIdAsync(string userId, string deckId);
}
