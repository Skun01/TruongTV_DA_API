using Domain.Entities;

namespace Application.IRepositories;

public interface IDeckTypeRepository : IRepository<DeckType>
{
    Task<List<DeckType>> GetAllOrderedAsync();
    Task<(List<DeckType> Items, int Total)> SearchAsync(string? query, int page, int pageSize);
    Task<bool> ExistsByNameAsync(string name, string? excludeId = null);
}
