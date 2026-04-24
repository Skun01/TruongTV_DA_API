using Application.Common;
using Application.DTOs.Shadowing;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/shadowing")]
public class ShadowingController : BaseController
{
    private readonly IShadowingService _shadowingService;

    public ShadowingController(IShadowingService shadowingService)
    {
        _shadowingService = shadowingService;
    }

    [HttpGet("topics")]
    public async Task<ApiResponse<List<ShadowingTopicListItemResponse>>> SearchTopics([FromQuery] ShadowingTopicListQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _shadowingService.SearchTopicsAsync(query, userId);
        return ApiResponse<List<ShadowingTopicListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("topics/{topicId}")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> GetTopicDetail([FromRoute] string topicId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetTopicDetailAsync(topicId, userId);
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    [HttpPost("attempts")]
    public async Task<ApiResponse<ShadowingAttemptResponse>> SubmitAttempt(
        [FromForm] SubmitShadowingAttemptFormRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await using var stream = request.Audio.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var result = await _shadowingService.SubmitAttemptAsync(userId, new SubmitShadowingAttemptRequest
        {
            TopicId = request.TopicId,
            SentenceId = request.SentenceId,
            Locale = string.IsNullOrWhiteSpace(request.Locale) ? "ja-JP" : request.Locale.Trim(),
            FileName = request.Audio.FileName,
            ContentType = request.Audio.ContentType,
            SizeInBytes = request.Audio.Length,
            AudioBytes = memory.ToArray(),
        }, cancellationToken);

        return ApiResponse<ShadowingAttemptResponse>.SuccessResponse(result);
    }

    [HttpGet("attempts/history")]
    public async Task<ApiResponse<List<ShadowingAttemptHistoryItemResponse>>> GetAttemptHistory([FromQuery] ShadowingAttemptHistoryQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _shadowingService.GetAttemptHistoryAsync(query, userId);
        return ApiResponse<List<ShadowingAttemptHistoryItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("sentences/{sentenceId}/progress")]
    public async Task<ApiResponse<ShadowingSentenceProgressResponse>> GetSentenceProgress([FromRoute] string sentenceId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetSentenceProgressAsync(sentenceId, userId);
        return ApiResponse<ShadowingSentenceProgressResponse>.SuccessResponse(result);
    }
}
