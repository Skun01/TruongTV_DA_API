using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class VocabularyDetailRepository : Repository<VocabularyDetail>, IVocabularyDetailRepository
{
    public VocabularyDetailRepository(AppDbContext context) : base(context)
    {
    }
}
