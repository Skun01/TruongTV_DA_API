using Domain.Entities;

namespace Application.IRepositories;

public interface IExtractedVocabularyRepository : IRepository<ExtractedVocabulary>
{
    Task<List<ExtractedVocabulary>> GetByMessageIdAsync(string messageId);
}
