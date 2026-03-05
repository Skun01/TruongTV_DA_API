using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DeckQueueRepository : Repository<DeckQueue>, IDeckQueueRepository
{
    public DeckQueueRepository(AppDbContext context) : base(context) {}

    public async Task<IEnumerable<DeckQueue>> GetByUserIdAsync(string userId)
    {
        return await _context.DeckQueues
            .AsNoTracking()
            .Where(q => q.UserId == userId)
            .Include(q => q.Deck)
                .ThenInclude(d => d.VocabularyCards)
            .Include(q => q.Deck)
                .ThenInclude(d => d.GrammarCards)
            .OrderByDescending(q => q.CreatedAt)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<DeckQueue?> GetByUserAndDeckAsync(string userId, string deckId)
    {
        return await _context.DeckQueues
            .FirstOrDefaultAsync(q => q.UserId == userId && q.DeckId == deckId);
    }

    public async Task<bool> IsExist(string userId, string deckId)
    {
        return await _context.DeckQueues
            .AnyAsync(q => q.UserId == userId && q.DeckId == deckId);
    }
}
