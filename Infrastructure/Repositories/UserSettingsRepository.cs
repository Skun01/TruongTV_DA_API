using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserSettingsRepository : Repository<UserSettings>, IUserSettingsRepository
{
    public UserSettingsRepository(AppDbContext context) : base(context) {}

    public async Task<UserSettings?> GetByUserIdAsync(string userId)
    {
        return await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
