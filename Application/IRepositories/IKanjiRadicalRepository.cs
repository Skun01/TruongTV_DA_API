using Domain.Entities;

namespace Application.IRepositories;

public interface IKanjiRadicalRepository : IRepository<KanjiRadical>
{
    Task<List<KanjiRadical>> GetByKanjiIdAsync(string kanjiId);
}
