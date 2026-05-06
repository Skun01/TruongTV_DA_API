using Domain.Entities;

namespace Application.IRepositories;

public interface IConversationSessionRepository : IRepository<ConversationSession>
{
    Task<ConversationSession?> GetByIdWithMessagesAsync(string id);
    Task<List<ConversationSession>> GetByUserIdAsync(string userId, int page, int pageSize);
    Task<int> CountByUserIdAsync(string userId);
}
