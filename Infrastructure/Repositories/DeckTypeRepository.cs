using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DeckTypeRepository : Repository<DeckType>, IDeckTypeRepository
{
    public DeckTypeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<DeckType>> GetAllOrderedAsync()
    {
        return await _context.DeckTypes
            .AsNoTracking()
            .OrderBy(dt => dt.Name)
            .ToListAsync();
    }
}
