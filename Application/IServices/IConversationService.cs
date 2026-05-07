using Application.Common;
using Application.DTOs.Conversations;
using Application.DTOs.Conversations.Queries;

namespace Application.IServices;

public interface IConversationService
{
    ScenarioListResponse GetScenarios();
    Task<ConversationDetailResponse> GetConversationAsync(string conversationId, string userId, CancellationToken cancellationToken = default);
    Task<StartConversationResponse> StartConversationAsync(StartConversationRequest request, string userId, CancellationToken cancellationToken = default);
    Task<SendMessageResponse> SendMessageAsync(string conversationId, SendMessageRequest request, string userId, CancellationToken cancellationToken = default);
    Task<ConversationResultResponse> CompleteConversationAsync(string conversationId, string userId, CancellationToken cancellationToken = default);
    Task<ConversationResultResponse> GetResultAsync(string conversationId, string userId, CancellationToken cancellationToken = default);
    Task<(List<ConversationListItemResponse> Items, MetaData Meta)> SearchHistoryAsync(ConversationHistoryQuery query, string userId);
    Task DeleteConversationAsync(string conversationId, string userId);
}
