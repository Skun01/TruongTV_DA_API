using Domain.Entities;

namespace Application.IRepositories;

public interface IConversationMessageRepository : IRepository<ConversationMessage>
{
    Task<List<ConversationMessage>> GetByConversationIdAsync(string conversationId);
}
