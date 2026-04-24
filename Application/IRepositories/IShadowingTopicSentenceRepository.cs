using Domain.Entities;

namespace Application.IRepositories;

public interface IShadowingTopicSentenceRepository : IRepository<ShadowingTopicSentence>
{
    Task<List<ShadowingTopicSentence>> GetByTopicIdAsync(string topicId);
    Task<ShadowingTopicSentence?> GetByTopicAndSentenceIdAsync(string topicId, string sentenceId);
}
