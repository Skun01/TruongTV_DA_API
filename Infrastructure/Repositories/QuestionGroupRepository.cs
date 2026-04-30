using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class QuestionGroupRepository : Repository<QuestionGroup>, IQuestionGroupRepository
{
    public QuestionGroupRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<QuestionGroup?> GetWithQuestionsAsync(string id)
    {
        return await _context.QuestionGroups
            .Include(x => x.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
