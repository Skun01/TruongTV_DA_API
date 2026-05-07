using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConversationMessageRepository : Repository<ConversationMessage>, IConversationMessageRepository
{
    public ConversationMessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<ConversationMessage>> GetByConversationIdAsync(string conversationId)
    {
        return await dbSet
            .Include(x => x.NewVocabulary)
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.CreatedAt)
            .AsSplitQuery()
            .ToListAsync();
    }
}
