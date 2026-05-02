using Domain.Entities;

namespace Application.IRepositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeByUserIdAsync(string userId);
}
