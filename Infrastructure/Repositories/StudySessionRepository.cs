using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StudySessionRepository : Repository<StudySession>, IStudySessionRepository
{
    public StudySessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<StudySession?> GetByIdForUserAsync(string sessionId, string userId)
    {
        return await _context.StudySessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);
    }

    public async Task<List<StudySession>> GetRecentByUserAsync(string userId, int limit)
    {
        return await _context.StudySessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
