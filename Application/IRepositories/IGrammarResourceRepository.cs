using Domain.Entities;

namespace Application.IRepositories;

public interface IGrammarResourceRepository : IRepository<GrammarResource>
{
    Task<List<GrammarResource>> GetByCardIdAsync(string cardId);
}
