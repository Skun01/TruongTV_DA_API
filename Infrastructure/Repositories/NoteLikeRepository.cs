using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NoteLikeRepository : Repository<NoteLike>, INoteLikeRepository
{
    public NoteLikeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<NoteLike?> GetByUserAndNoteIdAsync(string userId, string noteId)
    {
        return await _context.NoteLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.NoteId == noteId);
    }
}
