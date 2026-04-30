using Domain.Entities;

namespace Application.IRepositories;

public interface IExamSectionRepository : IRepository<ExamSection>
{
    Task<ExamSection?> GetWithGroupsAsync(string id);
}
