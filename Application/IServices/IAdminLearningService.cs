using Application.Common;
using Application.DTOs.LearningAdmin;

namespace Application.IServices;

public interface IAdminLearningService
{
    Task<LearningAdminCardConfigResponse> GetCardConfigAsync(string cardId);
    Task<LearningAdminCardConfigResponse> UpdateCardConfigAsync(string cardId, UpdateLearningCardConfigRequest request);
    Task<LearningAdminCardSentenceConfigResponse> AttachSentenceAsync(string cardId, AttachLearningCardSentenceRequest request);
    Task<LearningAdminCardSentenceConfigResponse> UpdateSentenceConfigAsync(string cardId, string sentenceId, UpdateLearningCardSentenceConfigRequest request);
    Task<bool> DeleteSentenceAsync(string cardId, string sentenceId);
    Task<List<LearningAdminCardSentenceConfigResponse>> ReorderSentencesAsync(string cardId, ReorderLearningCardSentencesRequest request);
    Task<(List<LearningAdminCardIssueResponse> Items, MetaData Meta)> GetCardIssuesAsync(LearningAdminCardIssuesQuery query);
    Task<DeckLearningCoverageResponse> GetDeckCoverageAsync(string deckId);
    Task<LearningPreviewResponse> PreviewCardAsync(string cardId, LearningPreviewQuery query);
}
