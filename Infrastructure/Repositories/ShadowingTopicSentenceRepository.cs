using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShadowingTopicSentenceRepository : Repository<ShadowingTopicSentence>, IShadowingTopicSentenceRepository
{
    public ShadowingTopicSentenceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<ShadowingTopicSentence>> GetByTopicIdAsync(string topicId)
    {
        return await _context.ShadowingTopicSentences
            .Where(x => x.TopicId == topicId)
            .OrderBy(x => x.Position)
            .ToListAsync();
    }

    public async Task<ShadowingTopicSentence?> GetByTopicAndSentenceIdAsync(string topicId, string sentenceId)
    {
        return await _context.ShadowingTopicSentences
            .FirstOrDefaultAsync(x => x.TopicId == topicId && x.SentenceId == sentenceId);
    }
}
