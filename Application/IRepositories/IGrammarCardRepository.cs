using Domain.Entities;

namespace Application.IRepositories;

public interface IGrammarCardRepository : IRepository<GrammarCard>
{
    Task<IEnumerable<GrammarCard>> GetAllByDeckIdAsync(string deckId);
    Task<GrammarCard?> GetFullInfoByIdAsync(string id);
}
