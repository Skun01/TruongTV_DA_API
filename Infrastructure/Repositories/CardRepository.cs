using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Card?> GetVocabularyDetailByIdAsync(string cardId)
    {
        return await _context.Cards
            .AsNoTracking()
            .Include(c => c.VocabularyDetail)
            .Include(c => c.CardSentences)
                .ThenInclude(cs => cs.Sentence)
            .FirstOrDefaultAsync(c => c.Id == cardId);
    }
}
