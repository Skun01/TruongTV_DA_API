using Domain.Entities;

namespace Application.IRepositories;

public interface IGrammarRelationRepository : IRepository<GrammarRelation>
{
    Task<List<GrammarRelation>> GetByGrammarIdAsync(string grammarId);
}
