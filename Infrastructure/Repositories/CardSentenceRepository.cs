using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardSentenceRepository : Repository<CardSentence>, ICardSentenceRepository
{
    public CardSentenceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<CardSentence>> GetByCardIdAsync(string cardId)
    {
        return await _context.CardSentences
            .Where(cs => cs.CardId == cardId)
            .OrderBy(cs => cs.Position)
            .ToListAsync();
    }

    public async Task<CardSentence?> GetByCardAndSentenceIdAsync(string cardId, string sentenceId)
    {
        return await _context.CardSentences
            .FirstOrDefaultAsync(cs => cs.CardId == cardId && cs.SentenceId == sentenceId);
    }
}
