using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class GrammarRepository : Repository<GrammarCard>, IGrammarRepository
{
    public GrammarRepository(AppDbContext context) : base(context) {}
}
