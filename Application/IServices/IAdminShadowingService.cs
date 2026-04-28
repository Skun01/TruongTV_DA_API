using Application.Common;
using Application.DTOs.Shadowing;
using Application.DTOs.ShadowingAdmin;

namespace Application.IServices;

public interface IAdminShadowingService
{
    Task<(List<ShadowingTopicListItemResponse> Items, MetaData Meta)> SearchTopicsAsync(AdminShadowingTopicListQuery query);
    Task<ShadowingTopicDetailResponse> GetTopicDetailAsync(string topicId);
    Task<ShadowingTopicDetailResponse> CreateTopicAsync(CreateShadowingTopicRequest request, string currentUserId);
    Task<ShadowingTopicDetailResponse> UpdateTopicAsync(string topicId, UpdateShadowingTopicRequest request);
    Task<bool> DeleteTopicAsync(string topicId);
    Task<(List<AdminShadowingAvailableSentenceResponse> Items, MetaData Meta)> SearchAvailableSentencesAsync(string topicId, AdminShadowingAvailableSentenceQuery query, string currentUserId);
    Task<ShadowingTopicSentenceResponse> AttachSentenceAsync(string topicId, AttachShadowingTopicSentenceRequest request);
    Task<List<ShadowingTopicSentenceResponse>> BulkAttachSentencesAsync(string topicId, BulkAttachShadowingTopicSentencesRequest request);
    Task<ShadowingTopicSentenceResponse> UpdateTopicSentenceAsync(string topicId, string sentenceId, UpdateShadowingTopicSentenceRequest request);
    Task<bool> DeleteTopicSentenceAsync(string topicId, string sentenceId);
    Task<List<ShadowingTopicSentenceResponse>> ReorderTopicSentencesAsync(string topicId, ReorderShadowingTopicSentencesRequest request);
    Task<ShadowingTopicAnalyticsResponse> GetTopicAnalyticsAsync(string topicId);
    Task<List<ShadowingTopicSentenceAnalyticsResponse>> GetTopicSentenceAnalyticsAsync(string topicId);
}
