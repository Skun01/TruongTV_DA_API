using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardProgressRepository : Repository<CardProgress>, ICardProgressRepository
{
    public CardProgressRepository(AppDbContext context) : base(context) {}

    public async Task<CardProgress?> GetByUserAndCardAsync(string userId, string cardId, DeckType cardType)
    {
        return await _context.CardProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CardId == cardId && p.CardType == cardType);
    }

    public async Task<List<CardProgress>> GetByUserAndCardIdsAsync(string userId, IEnumerable<string> cardIds, DeckType cardType)
    {
        return await _context.CardProgresses
            .Where(p => p.UserId == userId && cardIds.Contains(p.CardId) && p.CardType == cardType)
            .ToListAsync();
    }

    public async Task<List<CardProgress>> GetDueForReviewAsync(string userId, IEnumerable<string> cardIds, DeckType cardType)
    {
        return await _context.CardProgresses
            .Where(p => p.UserId == userId 
                && cardIds.Contains(p.CardId) 
                && p.CardType == cardType
                && p.NextReviewAt != null 
                && p.NextReviewAt <= DateTime.UtcNow)
            .OrderBy(p => p.NextReviewAt)
            .ToListAsync();
    }

    public async Task<int> GetTodayLearnedCountAsync(string userId)
    {
        var todayUtc = DateTime.UtcNow.Date;
        return await _context.CardProgresses
            .CountAsync(p => p.UserId == userId && p.LearnedAt != null && p.LearnedAt.Value.Date == todayUtc);
    }

    public async Task<int> GetCountByStatusAsync(string userId, IEnumerable<string> cardIds, DeckType cardType, CardStatus status)
    {
        return await _context.CardProgresses
            .CountAsync(p => p.UserId == userId && cardIds.Contains(p.CardId) && p.CardType == cardType && p.Status == status);
    }

    public async Task<int> GetDueCountAsync(string userId, IEnumerable<string> cardIds, DeckType cardType)
    {
        return await _context.CardProgresses
            .CountAsync(p => p.UserId == userId 
                && cardIds.Contains(p.CardId) 
                && p.CardType == cardType
                && p.NextReviewAt != null 
                && p.NextReviewAt <= DateTime.UtcNow);
    }

    public async Task<(int correct, int total)> GetAccuracyAsync(string userId, IEnumerable<string> cardIds, DeckType cardType)
    {
        var progresses = await _context.CardProgresses
            .Where(p => p.UserId == userId && cardIds.Contains(p.CardId) && p.CardType == cardType && p.TotalReviews > 0)
            .Select(p => new { p.CorrectReviews, p.TotalReviews })
            .ToListAsync();

        var totalCorrect = progresses.Sum(p => p.CorrectReviews);
        var totalReviews = progresses.Sum(p => p.TotalReviews);

        return (totalCorrect, totalReviews);
    }
}
