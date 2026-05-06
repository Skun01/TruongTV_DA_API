using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConversationSessionRepository : Repository<ConversationSession>, IConversationSessionRepository
{
    public ConversationSessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ConversationSession?> GetByIdWithMessagesAsync(string id)
    {
        return await dbSet
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ConversationSession>> GetByUserIdAsync(string userId, int page, int pageSize)
    {
        return await dbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        return await dbSet.CountAsync(x => x.UserId == userId);
    }
}
