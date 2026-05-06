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

    [HttpGet("scenarios")]
    [AllowAnonymous]
    public ApiResponse<ScenarioListResponse> GetScenarios()
    {
        var result = _conversationService.GetScenarios();
        return ApiResponse<ScenarioListResponse>.SuccessResponse(result);
    }

    [HttpPost("start")]
    public async Task<ApiResponse<StartConversationResponse>> StartConversation(
        [FromBody] StartConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.StartConversationAsync(request, userId, cancellationToken);
        return ApiResponse<StartConversationResponse>.SuccessResponse(result);
    }

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

    [HttpGet("{conversationId}/result")]
    public async Task<ApiResponse<ConversationResultResponse>> GetResult(
        [FromRoute] string conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.GetResultAsync(conversationId, userId, cancellationToken);
        return ApiResponse<ConversationResultResponse>.SuccessResponse(result);
    }

    [HttpGet]
    public async Task<ApiResponse<List<ConversationListItemResponse>>> SearchHistory([FromQuery] ConversationHistoryQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _conversationService.SearchHistoryAsync(query, userId);
        return ApiResponse<List<ConversationListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpDelete("{conversationId}")]
    public async Task<ApiResponse<bool>> DeleteConversation([FromRoute] string conversationId)
    {
        var userId = GetCurrentUserId();
        await _conversationService.DeleteConversationAsync(conversationId, userId);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
