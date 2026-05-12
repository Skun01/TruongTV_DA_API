using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IQuestionRepository : IRepository<Question>
{
    Task<(List<Question> Items, int Total)> SearchAsync(
        string? keyword,
        JlptLevel? level,
        SectionType? sectionType,
        int page,
        int pageSize);

    Task<Question?> GetDetailByIdAsync(string id);
    Task<List<Question>> GetDuplicateCandidatesAsync(JlptLevel level, SectionType sectionType, int limit = 200);
}
