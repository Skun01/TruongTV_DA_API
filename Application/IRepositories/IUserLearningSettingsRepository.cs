using Domain.Entities;

namespace Application.IRepositories;

public interface IUserLearningSettingsRepository : IRepository<UserLearningSettings>
{
    Task<UserLearningSettings?> GetByUserIdAsync(string userId);
}
