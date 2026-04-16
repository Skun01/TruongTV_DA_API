using Domain.Entities;

namespace Application.IRepositories;

public interface IDeckTypeRepository : IRepository<DeckType>
{
    Task<List<DeckType>> GetAllOrderedAsync();
}
