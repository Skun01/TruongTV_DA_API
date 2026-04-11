using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GrammarRelationRepository : Repository<GrammarRelation>, IGrammarRelationRepository
{
    public GrammarRelationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<GrammarRelation>> GetByGrammarIdAsync(string grammarId)
    {
        return await _context.GrammarRelations
            .Where(r => r.GrammarId == grammarId)
            .Include(r => r.Related)
            .ToListAsync();
    }
}
