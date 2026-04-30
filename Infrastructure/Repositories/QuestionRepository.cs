using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Question> Items, int Total)> SearchAsync(
        string? keyword,
        JlptLevel? level,
        SectionType? sectionType,
        int page,
        int pageSize)
    {
        var query = _context.Questions
            .AsNoTracking()
            .Include(x => x.Options)
            .Include(x => x.Group)
                .ThenInclude(g => g.Section)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.QuestionText, pattern));
        }

        if (level.HasValue)
            query = query.Where(x => x.Group.Section.Exam.Level == level.Value);

        if (sectionType.HasValue)
            query = query.Where(x => x.Group.Section.SectionType == sectionType.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Question?> GetDetailByIdAsync(string id)
    {
        return await _context.Questions
            .AsNoTracking()
            .Include(x => x.Options)
            .Include(x => x.Group)
                .ThenInclude(g => g.Section)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
