using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AiGeneratedQuestionRepository : Repository<AiGeneratedQuestion>, IAiGeneratedQuestionRepository
{
    public AiGeneratedQuestionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<AiGeneratedQuestion> Items, int Total)> SearchAsync(
        JlptLevel? level,
        SectionType? sectionType,
        AiQuestionStatus? status,
        int page,
        int pageSize)
    {
        var query = _context.AiGeneratedQuestions
            .AsNoTracking()
            .Include(x => x.Creator)
            .Include(x => x.Reviewer)
            .AsQueryable();

        if (level.HasValue)
            query = query.Where(x => x.Level == level.Value);

        if (sectionType.HasValue)
            query = query.Where(x => x.SectionType == sectionType.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<AiGeneratedQuestion?> GetDetailByIdAsync(string id)
    {
        return await _context.AiGeneratedQuestions
            .Include(x => x.Creator)
            .Include(x => x.Reviewer)
            .Include(x => x.Question)
                .ThenInclude(q => q!.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
