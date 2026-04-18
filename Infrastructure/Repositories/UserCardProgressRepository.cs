using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserCardProgressRepository : Repository<UserCardProgress>, IUserCardProgressRepository
{
    public UserCardProgressRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<UserCardProgress?> GetByUserAndCardIdAsync(string userId, string cardId)
    {
        return await _context.UserCardProgresses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CardId == cardId);
    }

    public async Task<List<UserCardProgress>> GetDueByUserAndCardIdsAsync(string userId, List<string> cardIds, DateTime reviewAt)
    {
        return await _context.UserCardProgresses
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && cardIds.Contains(x.CardId)
                && !x.IsMastered
                && x.SrsLevel != SrsLevel.level_12
                && x.NextReviewAt <= reviewAt)
            .ToListAsync();
    }

    public async Task<List<UserCardProgress>> GetDueByUserAsync(string userId, DateTime reviewAt)
    {
        return await _context.UserCardProgresses
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && !x.IsMastered
                && x.SrsLevel != SrsLevel.level_12
                && x.NextReviewAt <= reviewAt)
            .ToListAsync();
    }
}
