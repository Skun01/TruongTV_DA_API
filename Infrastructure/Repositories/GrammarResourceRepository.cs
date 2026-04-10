using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GrammarResourceRepository : Repository<GrammarResource>, IGrammarResourceRepository
{
    public GrammarResourceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<GrammarResource>> GetByCardIdAsync(string cardId)
    {
        return await _context.GrammarResources
            .Where(r => r.CardId == cardId)
            .ToListAsync();
    }
}
