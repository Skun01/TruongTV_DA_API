using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IExamRepository : IRepository<Exam>
{
    Task<(List<Exam> Items, int Total)> SearchAsync(
        string? keyword,
        JlptLevel? level,
        PublishStatus? status,
        int page,
        int pageSize);

    Task<Exam?> GetDetailByIdAsync(string id);
    Task<Exam?> GetWithSectionsAsync(string id);
}
