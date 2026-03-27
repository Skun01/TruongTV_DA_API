using Domain.Entities;

namespace Application.IRepositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailExist(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPasswordResetTokenAsync(string tokenHash);
}
