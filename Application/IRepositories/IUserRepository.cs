using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailExist(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPasswordResetTokenAsync(string tokenHash);
    Task<(List<User> Items, int Total)> SearchAsync(string? q, UserRole? role, bool? isActive, bool? isVerified, int page, int pageSize);
}
