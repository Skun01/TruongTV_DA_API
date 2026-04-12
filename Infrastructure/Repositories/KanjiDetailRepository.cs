using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class KanjiDetailRepository : Repository<KanjiDetail>, IKanjiDetailRepository
{
    public KanjiDetailRepository(AppDbContext context) : base(context)
    {
    }
}
