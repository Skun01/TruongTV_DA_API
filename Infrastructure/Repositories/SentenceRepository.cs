using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class SentenceRepository : Repository<Sentence>, ISentenceRepository
{
    public SentenceRepository(AppDbContext context) : base(context)
    {
    }
}
