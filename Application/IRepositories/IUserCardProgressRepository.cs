using Domain.Entities;

namespace Application.IRepositories;

public interface IUserCardProgressRepository : IRepository<UserCardProgress>
{
    Task<UserCardProgress?> GetByUserAndCardIdAsync(string userId, string cardId);
    Task<List<UserCardProgress>> GetDueByUserAndCardIdsAsync(string userId, List<string> cardIds, DateTime reviewAt);
    Task<List<UserCardProgress>> GetDueByUserAsync(string userId, DateTime reviewAt);
}
