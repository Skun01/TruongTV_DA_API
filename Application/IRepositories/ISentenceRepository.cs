using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface ISentenceRepository : IRepository<Sentence>
{
	Task<(List<Sentence> Items, int Total)> SearchAsync(string? query, JlptLevel? level, int page, int pageSize);
}
