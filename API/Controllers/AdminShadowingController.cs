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

    /// <summary>
    /// Tìm kiếm danh sách chủ đề shadowing (admin)
    /// </summary>
    [HttpGet("topics")]
    public async Task<ApiResponse<List<ShadowingTopicListItemResponse>>> SearchTopics([FromQuery] AdminShadowingTopicListQuery query)
    {
        var (items, meta) = await _adminShadowingService.SearchTopicsAsync(query);
        return ApiResponse<List<ShadowingTopicListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết chủ đề shadowing (admin)
    /// </summary>
    [HttpGet("topics/{topicId}")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> GetTopicDetail([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.GetTopicDetailAsync(topicId);
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tạo chủ đề shadowing mới (admin)
    /// </summary>
    [HttpPost("topics")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> CreateTopic([FromBody] CreateShadowingTopicRequest request)
    {
        var result = await _adminShadowingService.CreateTopicAsync(request, GetCurrentUserId());
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật chủ đề shadowing (admin)
    /// </summary>
    [HttpPatch("topics/{topicId}")]
    public async Task<ApiResponse<ShadowingTopicDetailResponse>> UpdateTopic([FromRoute] string topicId, [FromBody] UpdateShadowingTopicRequest request)
    {
        var result = await _adminShadowingService.UpdateTopicAsync(topicId, request);
        return ApiResponse<ShadowingTopicDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tìm kiếm câu có thể gắn vào chủ đề
    /// </summary>
    [HttpGet("topics/{topicId}/available-sentences")]
    public async Task<ApiResponse<List<AdminShadowingAvailableSentenceResponse>>> SearchAvailableSentences(
        [FromRoute] string topicId,
        [FromQuery] AdminShadowingAvailableSentenceQuery query)
    {
        var (items, meta) = await _adminShadowingService.SearchAvailableSentencesAsync(topicId, query, GetCurrentUserId());
        return ApiResponse<List<AdminShadowingAvailableSentenceResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Xóa chủ đề shadowing (admin)
    /// </summary>
    [HttpDelete("topics/{topicId}")]
    public async Task<ApiResponse<bool>> DeleteTopic([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.DeleteTopicAsync(topicId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Gắn câu vào chủ đề shadowing
    /// </summary>
    [HttpPost("topics/{topicId}/sentences")]
    public async Task<ApiResponse<ShadowingTopicSentenceResponse>> AttachSentence([FromRoute] string topicId, [FromBody] AttachShadowingTopicSentenceRequest request)
    {
        var result = await _adminShadowingService.AttachSentenceAsync(topicId, request);
        return ApiResponse<ShadowingTopicSentenceResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Gắn hàng loạt câu vào chủ đề shadowing
    /// </summary>
    [HttpPost("topics/{topicId}/sentences/bulk")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceResponse>>> BulkAttachSentences(
        [FromRoute] string topicId,
        [FromBody] BulkAttachShadowingTopicSentencesRequest request)
    {
        var result = await _adminShadowingService.BulkAttachSentencesAsync(topicId, request);
        return ApiResponse<List<ShadowingTopicSentenceResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật câu trong chủ đề shadowing
    /// </summary>
    [HttpPut("topics/{topicId}/sentences/{sentenceId}")]
    public async Task<ApiResponse<ShadowingTopicSentenceResponse>> UpdateSentence(
        [FromRoute] string topicId,
        [FromRoute] string sentenceId,
        [FromBody] UpdateShadowingTopicSentenceRequest request)
    {
        var result = await _adminShadowingService.UpdateTopicSentenceAsync(topicId, sentenceId, request);
        return ApiResponse<ShadowingTopicSentenceResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa câu khỏi chủ đề shadowing
    /// </summary>
    [HttpDelete("topics/{topicId}/sentences/{sentenceId}")]
    public async Task<ApiResponse<bool>> DeleteSentence([FromRoute] string topicId, [FromRoute] string sentenceId)
    {
        var result = await _adminShadowingService.DeleteTopicSentenceAsync(topicId, sentenceId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Sắp xếp lại thứ tự câu trong chủ đề shadowing
    /// </summary>
    [HttpPost("topics/{topicId}/sentences/reorder")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceResponse>>> ReorderSentences(
        [FromRoute] string topicId,
        [FromBody] ReorderShadowingTopicSentencesRequest request)
    {
        var result = await _adminShadowingService.ReorderTopicSentencesAsync(topicId, request);
        return ApiResponse<List<ShadowingTopicSentenceResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy thống kê chủ đề shadowing
    /// </summary>
    [HttpGet("topics/{topicId}/analytics")]
    public async Task<ApiResponse<ShadowingTopicAnalyticsResponse>> GetTopicAnalytics([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.GetTopicAnalyticsAsync(topicId);
        return ApiResponse<ShadowingTopicAnalyticsResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy thống kê từng câu trong chủ đề shadowing
    /// </summary>
    [HttpGet("topics/{topicId}/analytics/sentences")]
    public async Task<ApiResponse<List<ShadowingTopicSentenceAnalyticsResponse>>> GetTopicSentenceAnalytics([FromRoute] string topicId)
    {
        var result = await _adminShadowingService.GetTopicSentenceAnalyticsAsync(topicId);
        return ApiResponse<List<ShadowingTopicSentenceAnalyticsResponse>>.SuccessResponse(result);
    }
}
