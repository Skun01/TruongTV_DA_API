using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DeckBookmarkRepository : Repository<DeckBookmark>, IDeckBookmarkRepository
{
    public DeckBookmarkRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DeckBookmark?> GetByUserAndDeckIdAsync(string userId, string deckId)
    {
        return await _context.DeckBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.DeckId == deckId);
    }
}
