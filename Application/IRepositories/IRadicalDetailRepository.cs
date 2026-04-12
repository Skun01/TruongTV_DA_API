using Domain.Entities;

namespace Application.IRepositories;

public interface IRadicalDetailRepository : IRepository<RadicalDetail>
{
    Task<RadicalDetail?> GetByCharacterAsync(string character);
}
