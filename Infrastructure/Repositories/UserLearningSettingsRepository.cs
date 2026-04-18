using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserLearningSettingsRepository : Repository<UserLearningSettings>, IUserLearningSettingsRepository
{
    public UserLearningSettingsRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<UserLearningSettings?> GetByUserIdAsync(string userId)
    {
        return await _context.UserLearningSettings
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
