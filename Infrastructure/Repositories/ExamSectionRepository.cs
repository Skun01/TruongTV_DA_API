using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExamSectionRepository : Repository<ExamSection>, IExamSectionRepository
{
    public ExamSectionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ExamSection?> GetWithGroupsAsync(string id)
    {
        return await _context.ExamSections
            .Include(x => x.QuestionGroups.OrderBy(g => g.OrderIndex))
                .ThenInclude(g => g.Questions.OrderBy(q => q.OrderIndex))
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
