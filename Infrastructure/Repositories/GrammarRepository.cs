using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GrammarRepository : Repository<GrammarCard>, IGrammarRepository
{
    public GrammarRepository(AppDbContext context) : base(context) {}

    public async Task<IEnumerable<GrammarCard>> GetAllByDeckIdAsync(string deckId)
    {
        return await _context.GrammarCards.Where(gc => gc.DeckId == deckId)
            .ToListAsync();
    }
}
