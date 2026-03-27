using Application.IRepositories;
using Domain.Entities;
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
}
