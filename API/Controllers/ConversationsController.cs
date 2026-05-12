using Application.Common;
using Application.DTOs.Conversations;
using Application.DTOs.Conversations.Queries;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/conversations")]
public class ConversationsController : BaseController
{
    private readonly IConversationService _conversationService;

    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    /// <summary>
    /// Lấy danh sách kịch bản hội thoại
    /// </summary>
    [HttpGet("scenarios")]
    [AllowAnonymous]
    public ApiResponse<ScenarioListResponse> GetScenarios()
    {
        var result = _conversationService.GetScenarios();
        return ApiResponse<ScenarioListResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy chi tiết cuộc hội thoại
    /// </summary>
    [HttpGet("{conversationId}")]
    public async Task<ApiResponse<ConversationDetailResponse>> GetConversation(
        [FromRoute] string conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.GetConversationAsync(conversationId, userId, cancellationToken);
        return ApiResponse<ConversationDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Bắt đầu cuộc hội thoại mới
    /// </summary>
    [HttpPost("start")]
    public async Task<ApiResponse<StartConversationResponse>> StartConversation(
        [FromBody] StartConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.StartConversationAsync(request, userId, cancellationToken);
        return ApiResponse<StartConversationResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Gửi tin nhắn trong cuộc hội thoại
    /// </summary>
    [HttpPost("{conversationId}/message")]
    public async Task<ApiResponse<SendMessageResponse>> SendMessage(
        [FromRoute] string conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.SendMessageAsync(conversationId, request, userId, cancellationToken);
        return ApiResponse<SendMessageResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Kết thúc cuộc hội thoại và lấy đánh giá
    /// </summary>
    [HttpPost("{conversationId}/complete")]
    public async Task<ApiResponse<ConversationResultResponse>> CompleteConversation(
        [FromRoute] string conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.CompleteConversationAsync(conversationId, userId, cancellationToken);
        return ApiResponse<ConversationResultResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy kết quả đánh giá cuộc hội thoại
    /// </summary>
    [HttpGet("{conversationId}/result")]
    public async Task<ApiResponse<ConversationResultResponse>> GetResult(
        [FromRoute] string conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.GetResultAsync(conversationId, userId, cancellationToken);
        return ApiResponse<ConversationResultResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tìm kiếm lịch sử hội thoại
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<ConversationListItemResponse>>> SearchHistory([FromQuery] ConversationHistoryQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _conversationService.SearchHistoryAsync(query, userId);
        return ApiResponse<List<ConversationListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Xóa cuộc hội thoại
    /// </summary>
    [HttpDelete("{conversationId}")]
    public async Task<ApiResponse<bool>> DeleteConversation([FromRoute] string conversationId)
    {
        var userId = GetCurrentUserId();
        await _conversationService.DeleteConversationAsync(conversationId, userId);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
