using Application.Common;
using Application.DTOs.Shadowing;

namespace Application.IServices;

public interface IShadowingService
{
    Task<(List<ShadowingTopicListItemResponse> Items, MetaData Meta)> SearchTopicsAsync(ShadowingTopicListQuery query, string userId);
    Task<ShadowingTopicDetailResponse> GetTopicDetailAsync(string topicId, string userId);
    Task<ShadowingAttemptResponse> SubmitAttemptAsync(string userId, SubmitShadowingAttemptRequest request, CancellationToken cancellationToken = default);
    Task<ShadowingAttemptResponse> GetAttemptDetailAsync(string attemptId, string userId);
    Task<(List<ShadowingAttemptHistoryItemResponse> Items, MetaData Meta)> GetAttemptHistoryAsync(ShadowingAttemptHistoryQuery query, string userId);
    Task<ShadowingSentenceProgressResponse> GetSentenceProgressAsync(string sentenceId, string userId);
    Task<ShadowingTopicProgressResponse> GetTopicProgressAsync(string topicId, string userId);
    Task<List<ShadowingTopicSentenceProgressItemResponse>> GetTopicSentenceProgressAsync(string topicId, string userId);
    Task<ShadowingTopicResumeResponse> GetTopicResumeAsync(string topicId, string userId);
}
