using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExamSessionRepository : Repository<ExamSession>, IExamSessionRepository
{
    public ExamSessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ExamSession?> GetWithAnswersAsync(string id)
    {
        return await _context.ExamSessions
            .Include(x => x.Answers)
            .Include(x => x.Exam)
                .ThenInclude(e => e.Sections.OrderBy(s => s.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ExamSession?> GetFullDetailAsync(string id)
    {
        return await _context.ExamSessions
            .AsNoTracking()
            .Include(x => x.Exam)
                .ThenInclude(e => e.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.QuestionGroups.OrderBy(g => g.OrderIndex))
                        .ThenInclude(g => g.Questions.OrderBy(q => q.OrderIndex))
                            .ThenInclude(q => q.Options)
            .Include(x => x.Answers)
                .ThenInclude(a => a.SelectedOption)
            .Include(x => x.SectionScores)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ExamSession>> GetExpiredSessionsAsync()
    {
        return await _context.ExamSessions
            .Where(x => x.Status == ExamSessionStatus.InProgress)
            .Where(x => x.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<(List<ExamSession> Items, int Total)> SearchByUserAsync(
        string userId,
        string? examId,
        ExamSessionStatus? status,
        int page,
        int pageSize)
    {
        var query = _context.ExamSessions
            .AsNoTracking()
            .Include(x => x.Exam)
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(examId))
            query = query.Where(x => x.ExamId == examId);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
