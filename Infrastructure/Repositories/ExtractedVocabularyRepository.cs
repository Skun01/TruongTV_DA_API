using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExtractedVocabularyRepository : Repository<ExtractedVocabulary>, IExtractedVocabularyRepository
{
    public ExtractedVocabularyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<ExtractedVocabulary>> GetByMessageIdAsync(string messageId)
    {
        return await dbSet
            .Where(x => x.MessageId == messageId)
            .ToListAsync();
    }
}
