using Application.Common;
using Application.DTOs.Shadowing;
using Application.DTOs.ShadowingAdmin;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
[Route("api/admin/shadowing")]
public class AdminShadowingController : BaseController
{
    private readonly IAdminShadowingService _adminShadowingService;

    public AdminShadowingController(IAdminShadowingService adminShadowingService)
    {
        _adminShadowingService = adminShadowingService;
    }

    [HttpGet("topics")]
    public async Task<ApiResponse<List<ShadowingTopicListItemResponse>>> SearchTopics([FromQuery] AdminShadowingTopicListQuery query)
    {
        var (items, meta) = await _adminShadowingService.SearchTopicsAsync(query);
        return ApiResponse<List<ShadowingTopicListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("topics/{topicId}")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> GetTopicDetail([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.GetTopicDetailAsync(topicId);
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    [HttpPost("topics")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> CreateTopic([FromBody] CreateShadowingTopicRequest request)
    {
        var result = await _adminShadowingService.CreateTopicAsync(request, GetCurrentUserId());
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    [HttpPatch("topics/{topicId}")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> UpdateTopic([FromRoute] string topicId, [FromBody] UpdateShadowingTopicRequest request)
    {
        var result = await _adminShadowingService.UpdateTopicAsync(topicId, request);
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    [HttpGet("topics/{topicId}/available-sentences")]
    public async Task<ApiResponse<List<AdminShadowingAvailableSentenceResponse>>> SearchAvailableSentences(
        [FromRoute] string topicId,
        [FromQuery] AdminShadowingAvailableSentenceQuery query)
    {
        var (items, meta) = await _adminShadowingService.SearchAvailableSentencesAsync(topicId, query, GetCurrentUserId());
        return ApiResponse<List<AdminShadowingAvailableSentenceResponse>>.SuccessResponse(items, meta);
    }

    [HttpDelete("topics/{topicId}")]
    public async Task<ApiResponse<bool>> DeleteTopic([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.DeleteTopicAsync(topicId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpPost("topics/{topicId}/sentences")]
    public async Task<ApiResponse<ShadowingTopicSentenceResponse>> AttachSentence([FromRoute] string topicId, [FromBody] AttachShadowingTopicSentenceRequest request)
    {
        var result = await _adminShadowingService.AttachSentenceAsync(topicId, request);
        return ApiResponse<ShadowingTopicSentenceResponse>.SuccessResponse(result);
    }

    [HttpPost("topics/{topicId}/sentences/bulk")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceResponse>>> BulkAttachSentences(
        [FromRoute] string topicId,
        [FromBody] BulkAttachShadowingTopicSentencesRequest request)
    {
        var result = await _adminShadowingService.BulkAttachSentencesAsync(topicId, request);
        return ApiResponse<List<ShadowingTopicSentenceResponse>>.SuccessResponse(result);
    }

    [HttpPut("topics/{topicId}/sentences/{sentenceId}")]
    public async Task<ApiResponse<ShadowingTopicSentenceResponse>> UpdateSentence(
        [FromRoute] string topicId,
        [FromRoute] string sentenceId,
        [FromBody] UpdateShadowingTopicSentenceRequest request)
    {
        var result = await _adminShadowingService.UpdateTopicSentenceAsync(topicId, sentenceId, request);
        return ApiResponse<ShadowingTopicSentenceResponse>.SuccessResponse(result);
    }

    [HttpDelete("topics/{topicId}/sentences/{sentenceId}")]
    public async Task<ApiResponse<bool>> DeleteSentence([FromRoute] string topicId, [FromRoute] string sentenceId)
    {
        var result = await _adminShadowingService.DeleteTopicSentenceAsync(topicId, sentenceId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpPost("topics/{topicId}/sentences/reorder")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceResponse>>> ReorderSentences(
        [FromRoute] string topicId,
        [FromBody] ReorderShadowingTopicSentencesRequest request)
    {
        var result = await _adminShadowingService.ReorderTopicSentencesAsync(topicId, request);
        return ApiResponse<List<ShadowingTopicSentenceResponse>>.SuccessResponse(result);
    }

    [HttpGet("topics/{topicId}/analytics")]
    public async Task<ApiResponse<ShadowingTopicAnalyticsResponse>> GetTopicAnalytics([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.GetTopicAnalyticsAsync(topicId);
        return ApiResponse<ShadowingTopicAnalyticsResponse>.SuccessResponse(result);
    }

    [HttpGet("topics/{topicId}/analytics/sentences")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceAnalyticsResponse>>> GetTopicSentenceAnalytics([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.GetTopicSentenceAnalyticsAsync(topicId);
        return ApiResponse<List<ShadowingTopicSentenceAnalyticsResponse>>.SuccessResponse(result);
    }
}
