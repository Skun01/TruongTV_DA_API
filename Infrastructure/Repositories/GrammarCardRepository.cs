using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GrammarCardRepository : Repository<GrammarCard>, IGrammarCardRepository
{
    public GrammarCardRepository(AppDbContext context) : base(context) {}

    public async Task<IEnumerable<GrammarCard>> GetAllByDeckIdAsync(string deckId)
    {
        return await _context.GrammarCards
            .AsNoTracking()
            .Where(gc => gc.DeckId == deckId)
            .ToListAsync();
    }

    public async Task<GrammarCard?> GetFullInfoByIdAsync(string id)
    {
        return await _context.GrammarCards
            .Include(gc => gc.Deck)
            .Include(gc => gc.ExampleSentences)
            .Where(gc => gc.Id == id)
            .FirstOrDefaultAsync();
    }
}
