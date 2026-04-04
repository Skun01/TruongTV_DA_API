using Application.Common;
using Application.DTOs.Sentences;

namespace Application.IServices;

public interface ISentenceService
{
    Task<SentenceResponse> CreateAsync(CreateSentenceRequest request);
    Task<SentenceResponse> GetByIdAsync(string id);
    Task<(List<SentenceResponse> Items, MetaData Meta)> SearchAsync(string? q, string? level, int page, int pageSize);
    Task<SentenceResponse> UpdateAsync(string id, UpdateSentenceRequest request);
    Task<bool> DeleteAsync(string id);
}
