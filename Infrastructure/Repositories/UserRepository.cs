using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) {}

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string tokenHash)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == tokenHash);
    }

    public async Task<bool> IsEmailExist(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<(List<User> Items, int Total)> SearchAsync(
        string? q,
        UserRole? role,
        bool? isActive,
        bool? isVerified,
        int page,
        int pageSize)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.Trim();
            query = query.Where(u =>
                EF.Functions.ILike(u.Email, $"%{keyword}%")
                || EF.Functions.ILike(u.Username, $"%{keyword}%"));
        }

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (isVerified.HasValue)
            query = query.Where(u => u.IsVerified == isVerified.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
