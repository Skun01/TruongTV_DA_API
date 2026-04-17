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

    public async Task<(List<DeckType> Items, int Total)> SearchAsync(string? query, int page, int pageSize)
    {
        var deckTypeQuery = _context.DeckTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            deckTypeQuery = deckTypeQuery.Where(dt => EF.Functions.ILike(dt.Name, pattern));
        }

        var total = await deckTypeQuery.CountAsync();
        var items = await deckTypeQuery
            .OrderBy(dt => dt.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> ExistsByNameAsync(string name, string? excludeId = null)
    {
        var normalizedName = name.Trim();

        return await _context.DeckTypes.AnyAsync(dt =>
            (excludeId == null || dt.Id != excludeId)
            && EF.Functions.ILike(dt.Name, normalizedName));
    }
}
