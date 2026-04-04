using Application.Common;
using Application.DTOs.Sentences;

namespace Application.IServices;

public interface ISentenceService
{
    Task<SentenceResponse> CreateAsync(CreateSentenceRequest request);
    Task<SentenceResponse> GetByIdAsync(string id);
    Task<(List<SentenceResponse> Items, MetaData Meta)> SearchAsync(SentenceSearchQuery query);
    Task<SentenceResponse> UpdateAsync(string id, UpdateSentenceRequest request);
    Task<bool> DeleteAsync(string id);
}
