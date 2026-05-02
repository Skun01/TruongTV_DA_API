using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) {}

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task RevokeByUserIdAsync(string userId)
    {
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.Revoked)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Revoked, true)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
    }
}
