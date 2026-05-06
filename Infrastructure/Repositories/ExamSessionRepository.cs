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
            .AsSplitQuery()
            .Include(x => x.Exam)
                .ThenInclude(e => e.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.QuestionGroups.OrderBy(g => g.OrderIndex))
                        .ThenInclude(g => g.Questions.OrderBy(q => q.OrderIndex))
                            .ThenInclude(q => q.Options)
            .Include(x => x.Answers)
                .ThenInclude(a => a.SelectedOption)
            .Include(x => x.SectionScores)
                .ThenInclude(ss => ss.Section)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ExamSession?> GetActiveSessionByExamAsync(string userId, string examId)
    {
        return await _context.ExamSessions
            .AsNoTracking()
            .Include(x => x.Exam)
                .ThenInclude(e => e.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.QuestionGroups.OrderBy(g => g.OrderIndex))
                        .ThenInclude(g => g.Questions.OrderBy(q => q.OrderIndex))
                            .ThenInclude(q => q.Options)
            .Include(x => x.Answers)
            .Where(x => x.UserId == userId)
            .Where(x => x.ExamId == examId)
            .Where(x => x.Status == ExamSessionStatus.InProgress)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync();
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

    public async Task<List<ExamSession>> GetRecentByUserAsync(string userId, int limit)
    {
        return await _context.ExamSessions
            .AsNoTracking()
            .Include(x => x.Exam)
                .ThenInclude(x => x.Sections)
            .Include(x => x.SectionScores)
            .Where(x => x.UserId == userId && x.Status == ExamSessionStatus.Submitted)
            .OrderByDescending(x => x.SubmittedAt ?? x.StartedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<(int TotalExamsTaken, int TotalPassed, int TotalFailed, double AverageScore, double PassRate)> GetHistoryStatsByUserAsync(string userId)
    {
        var submittedSessions = _context.ExamSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == ExamSessionStatus.Submitted);

        var totalExamsTaken = await submittedSessions.CountAsync();
        if (totalExamsTaken == 0)
            return (0, 0, 0, 0, 0);

        var totalPassed = await submittedSessions.CountAsync(x => x.IsPassed == true);
        var averageScore = await submittedSessions.AverageAsync(x => (double?)(x.TotalScore ?? 0)) ?? 0;
        var totalFailed = totalExamsTaken - totalPassed;
        var passRate = (double)totalPassed / totalExamsTaken * 100;

        return (
            totalExamsTaken,
            totalPassed,
            totalFailed,
            Math.Round(averageScore, 2),
            Math.Round(passRate, 2));
    }
}
