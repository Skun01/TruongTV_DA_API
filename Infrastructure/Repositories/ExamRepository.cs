using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExamRepository : Repository<Exam>, IExamRepository
{
    public ExamRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Exam> Items, int Total)> SearchAsync(
        string? keyword,
        JlptLevel? level,
        PublishStatus? status,
        int page,
        int pageSize)
    {
        var query = _context.Exams
            .AsNoTracking()
            .Include(x => x.Creator)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, pattern));
        }

        if (level.HasValue)
            query = query.Where(x => x.Level == level.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Exam> Items, int Total)> SearchPublishedAsync(
        string? keyword,
        JlptLevel? level,
        int page,
        int pageSize)
    {
        var query = _context.Exams
            .AsNoTracking()
            .Where(x => x.Status == PublishStatus.Published)
            .Include(x => x.Sections)
                .ThenInclude(s => s.QuestionGroups)
                    .ThenInclude(g => g.Questions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, pattern));
        }

        if (level.HasValue)
            query = query.Where(x => x.Level == level.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Exam?> GetDetailByIdAsync(string id)
    {
        return await _context.Exams
            .AsNoTracking()
            .Include(x => x.Creator)
            .Include(x => x.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.QuestionGroups.OrderBy(g => g.OrderIndex))
                    .ThenInclude(g => g.Questions.OrderBy(q => q.OrderIndex))
                        .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Exam?> GetPublishedDetailByIdAsync(string id)
    {
        return await _context.Exams
            .AsNoTracking()
            .Where(x => x.Status == PublishStatus.Published)
            .Include(x => x.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.QuestionGroups.OrderBy(g => g.OrderIndex))
                    .ThenInclude(g => g.Questions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Exam?> GetWithSectionsAsync(string id)
    {
        return await _context.Exams
            .Include(x => x.Sections.OrderBy(s => s.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
