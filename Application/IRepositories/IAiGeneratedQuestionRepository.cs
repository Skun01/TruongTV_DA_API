using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IAiGeneratedQuestionRepository : IRepository<AiGeneratedQuestion>
{
    Task<(List<AiGeneratedQuestion> Items, int Total)> SearchAsync(
        JlptLevel? level,
        SectionType? sectionType,
        AiQuestionStatus? status,
        int page,
        int pageSize);

    Task<AiGeneratedQuestion?> GetDetailByIdAsync(string id);
}
