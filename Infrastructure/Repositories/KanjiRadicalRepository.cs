using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class KanjiRadicalRepository : Repository<KanjiRadical>, IKanjiRadicalRepository
{
    public KanjiRadicalRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<KanjiRadical>> GetByKanjiIdAsync(string kanjiId)
    {
        return await _context.KanjiRadicals
            .Include(k => k.Radical)
            .Where(k => k.KanjiId == kanjiId)
            .ToListAsync();
    }
}
