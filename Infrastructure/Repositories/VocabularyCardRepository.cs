using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VocabularyCardRepository : Repository<VocabularyCard>, IVocabularyCardRepository
{
    public VocabularyCardRepository(AppDbContext context) : base(context) {}

    public async Task<IEnumerable<VocabularyCard>> GetAllByDeckId(string deckId)
    {
        return await _context.VocabularyCards.AsNoTracking()
            .Where(d => d.DeckId == deckId)
            .Include(d => d.ExampleSentences)
            .ToListAsync();
    }

    public async Task<VocabularyCard?> GetFullInfoByIdAsync(string id)
    {
        return await _context.VocabularyCards
            .Include(c => c.ExampleSentences)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
