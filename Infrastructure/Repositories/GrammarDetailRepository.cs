using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class GrammarDetailRepository : Repository<GrammarDetail>, IGrammarDetailRepository
{
    public GrammarDetailRepository(AppDbContext context) : base(context)
    {
    }
}
