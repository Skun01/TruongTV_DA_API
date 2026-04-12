using Application.Common;
using Application.DTOs.Kanji;

namespace Application.IServices;

public interface IKanjiService
{
    Task<KanjiDetailResponse> GetDetailAsync(string cardId, string? currentUserId);
    Task<(List<KanjiListItemResponse> Items, MetaData Meta)> SearchAsync(KanjiSearchQuery query, string currentUserId);
    Task<ImportKanjiRequest> GetImportTemplateAsync();
    Task<ImportKanjiRequest> ExportAsync(KanjiExportQuery query, string currentUserId);
    Task<KanjiImportPreviewResponse> PreviewImportAsync(ImportKanjiRequest request);
    Task<KanjiImportCommitResponse> CommitImportAsync(ImportKanjiRequest request, string currentUserId);
    Task<KanjiDetailResponse> CreateAsync(CreateKanjiCardRequest request, string currentUserId);
    Task<KanjiDetailResponse> UpdateAsync(string cardId, UpdateKanjiCardRequest request, string currentUserId);
    Task<bool> SoftDeleteAsync(string cardId, string currentUserId);
}
