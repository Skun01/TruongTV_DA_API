using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserCardNoteRepository : Repository<UserCardNote>, IUserCardNoteRepository
{
    public UserCardNoteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<UserCardNote>> GetByCardIdWithRelationsAsync(string cardId)
    {
        return await _context.UserCardNotes
            .AsNoTracking()
            .Include(n => n.User)
            .Include(n => n.NoteLikes)
            .Where(n => n.CardId == cardId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<UserCardNote> Notes, int Total)> GetCommunityByCardIdAsync(string cardId, string currentUserId, int page, int pageSize)
    {
        var query = _context.UserCardNotes
            .AsNoTracking()
            .Include(n => n.User)
            .Include(n => n.NoteLikes)
            .Where(n => n.CardId == cardId && n.UserId != currentUserId)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var notes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (notes, total);
    }

    public async Task<UserCardNote?> GetMyNoteByCardIdAsync(string cardId, string userId)
    {
        return await _context.UserCardNotes
            .Include(n => n.User)
            .Include(n => n.NoteLikes)
            .FirstOrDefaultAsync(n => n.CardId == cardId && n.UserId == userId);
    }

    public async Task<UserCardNote?> GetByIdWithRelationsAsync(string noteId)
    {
        return await _context.UserCardNotes
            .Include(n => n.User)
            .Include(n => n.NoteLikes)
            .FirstOrDefaultAsync(n => n.Id == noteId);
    }
}
