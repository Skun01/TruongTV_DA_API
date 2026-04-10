using Application.Common;
using Application.DTOs.Sentences;

namespace Application.IServices;

public interface ISentenceService
{
    Task<ImportSentenceRequest> GetImportTemplateAsync();
    Task<ImportSentenceRequest> ExportAsync(SentenceExportQuery query, string currentUserId);
    Task<SentenceImportPreviewResponse> PreviewImportAsync(ImportSentenceRequest request);
    Task<SentenceImportCommitResponse> CommitImportAsync(ImportSentenceRequest request, string currentUserId);
    Task<SentenceResponse> CreateAsync(CreateSentenceRequest request, string currentUserId);
    Task<SentenceResponse> GetByIdAsync(string id);
    Task<(List<SentenceResponse> Items, MetaData Meta)> SearchAsync(SentenceSearchQuery query, string currentUserId);
    Task<SentenceResponse> UpdateAsync(string id, UpdateSentenceRequest request);
    Task<bool> DeleteAsync(string id);
}
