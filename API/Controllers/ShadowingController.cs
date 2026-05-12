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

    /// <summary>
    /// Tìm kiếm danh sách chủ đề shadowing
    /// </summary>
    [HttpGet("topics")]
    public async Task<ApiResponse<List<ShadowingTopicListItemResponse>>> SearchTopics([FromQuery] ShadowingTopicListQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _shadowingService.SearchTopicsAsync(query, userId);
        return ApiResponse<List<ShadowingTopicListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết chủ đề shadowing
    /// </summary>
    [HttpGet("topics/{topicId}")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> GetTopicDetail([FromRoute] string topicId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetTopicDetailAsync(topicId, userId);
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy tiến độ học chủ đề shadowing
    /// </summary>
    [HttpGet("topics/{topicId}/progress")]
    public async Task<ApiResponse<ShadowingTopicProgressResponse>> GetTopicProgress([FromRoute] string topicId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetTopicProgressAsync(topicId, userId);
        return ApiResponse<ShadowingTopicProgressResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy tiến độ từng câu trong chủ đề shadowing
    /// </summary>
    [HttpGet("topics/{topicId}/sentences/progress")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceProgressItemResponse>>> GetTopicSentenceProgress([FromRoute] string topicId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetTopicSentenceProgressAsync(topicId, userId);
        return ApiResponse<List<ShadowingTopicSentenceProgressItemResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy vị trí tiếp tục học shadowing
    /// </summary>
    [HttpGet("topics/{topicId}/resume")]
    public async Task<ApiResponse<ShadowingTopicResumeResponse>> GetTopicResume([FromRoute] string topicId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetTopicResumeAsync(topicId, userId);
        return ApiResponse<ShadowingTopicResumeResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Nộp bài luyện shadowing
    /// </summary>
    [HttpPost("attempts")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ApiResponse<ShadowingAttemptResponse>> SubmitAttempt(
        [FromForm] SubmitShadowingAttemptFormRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await using var memoryStream = new MemoryStream();
        await request.Audio.CopyToAsync(memoryStream, cancellationToken);

        var result = await _shadowingService.SubmitAttemptAsync(userId, new SubmitShadowingAttemptRequest
        {
            TopicId = request.TopicId,
            SentenceId = request.SentenceId,
            Locale = string.IsNullOrWhiteSpace(request.Locale) ? "ja-JP" : request.Locale.Trim(),
            FileName = request.Audio.FileName,
            ContentType = request.Audio.ContentType,
            SizeInBytes = request.Audio.Length,
            AudioBytes = memoryStream.ToArray(),
        }, cancellationToken);

        return ApiResponse<ShadowingAttemptResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy chi tiết lần luyện shadowing
    /// </summary>
    [HttpGet("attempts/{attemptId}")]
    public async Task<ApiResponse<ShadowingAttemptResponse>> GetAttemptDetail([FromRoute] string attemptId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetAttemptDetailAsync(attemptId, userId);
        return ApiResponse<ShadowingAttemptResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy lịch sử luyện shadowing
    /// </summary>
    [HttpGet("attempts/history")]
    public async Task<ApiResponse<List<ShadowingAttemptHistoryItemResponse>>> GetAttemptHistory([FromQuery] ShadowingAttemptHistoryQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _shadowingService.GetAttemptHistoryAsync(query, userId);
        return ApiResponse<List<ShadowingAttemptHistoryItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy tiến độ luyện theo câu
    /// </summary>
    [HttpGet("sentences/{sentenceId}/progress")]
    public async Task<ApiResponse<ShadowingSentenceProgressResponse>> GetSentenceProgress([FromRoute] string sentenceId)
    {
        var userId = GetCurrentUserId();
        var result = await _shadowingService.GetSentenceProgressAsync(sentenceId, userId);
        return ApiResponse<ShadowingSentenceProgressResponse>.SuccessResponse(result);
    }
}
