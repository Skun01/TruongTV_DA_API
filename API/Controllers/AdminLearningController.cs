using Application.Common;
using Application.DTOs.LearningAdmin;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/learning")]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
public class AdminLearningController : BaseController
{
    private readonly IAdminLearningService _adminLearningService;

    public AdminLearningController(IAdminLearningService adminLearningService)
    {
        _adminLearningService = adminLearningService;
    }

    /// <summary>
    /// Lấy toàn bộ learning config của một card cho màn hình admin.
    /// </summary>
    [HttpGet("cards/{cardId}/config")]
    public async Task<ApiResponse<LearningAdminCardConfigResponse>> GetCardConfig([FromRoute] string cardId)
    {
        var result = await _adminLearningService.GetCardConfigAsync(cardId);
        return ApiResponse<LearningAdminCardConfigResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật learning config của một card, bao gồm summary và metadata của card sentences.
    /// </summary>
    [HttpPut("cards/{cardId}/config")]
    public async Task<ApiResponse<LearningAdminCardConfigResponse>> UpdateCardConfig(
        [FromRoute] string cardId,
        [FromBody] UpdateLearningCardConfigRequest request)
    {
        var result = await _adminLearningService.UpdateCardConfigAsync(cardId, request);
        return ApiResponse<LearningAdminCardConfigResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Gắn thêm một sentence vào card cùng metadata learning.
    /// </summary>
    [HttpPost("cards/{cardId}/sentences")]
    public async Task<ApiResponse<LearningAdminCardSentenceConfigResponse>> AttachSentence(
        [FromRoute] string cardId,
        [FromBody] AttachLearningCardSentenceRequest request)
    {
        var result = await _adminLearningService.AttachSentenceAsync(cardId, request);
        return ApiResponse<LearningAdminCardSentenceConfigResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật metadata learning của một sentence đã gắn vào card.
    /// </summary>
    [HttpPut("cards/{cardId}/sentences/{sentenceId}")]
    public async Task<ApiResponse<LearningAdminCardSentenceConfigResponse>> UpdateSentenceConfig(
        [FromRoute] string cardId,
        [FromRoute] string sentenceId,
        [FromBody] UpdateLearningCardSentenceConfigRequest request)
    {
        var result = await _adminLearningService.UpdateSentenceConfigAsync(cardId, sentenceId, request);
        return ApiResponse<LearningAdminCardSentenceConfigResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Gỡ một sentence khỏi card.
    /// </summary>
    [HttpDelete("cards/{cardId}/sentences/{sentenceId}")]
    public async Task<ApiResponse<bool>> DeleteSentence([FromRoute] string cardId, [FromRoute] string sentenceId)
    {
        var result = await _adminLearningService.DeleteSentenceAsync(cardId, sentenceId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật thứ tự hiển thị sentence của một card.
    /// </summary>
    [HttpPost("cards/{cardId}/sentences/reorder")]
    public async Task<ApiResponse<List<LearningAdminCardSentenceConfigResponse>>> ReorderSentences(
        [FromRoute] string cardId,
        [FromBody] ReorderLearningCardSentencesRequest request)
    {
        var result = await _adminLearningService.ReorderSentencesAsync(cardId, request);
        return ApiResponse<List<LearningAdminCardSentenceConfigResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Liệt kê các card đang có vấn đề về dữ liệu learning.
    /// </summary>
    [HttpGet("cards/issues")]
    public async Task<ApiResponse<List<LearningAdminCardIssueResponse>>> GetCardIssues([FromQuery] LearningAdminCardIssuesQuery query)
    {
        var (items, meta) = await _adminLearningService.GetCardIssuesAsync(query);
        return ApiResponse<List<LearningAdminCardIssueResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy thống kê độ phủ learning của một deck.
    /// </summary>
    [HttpGet("decks/{deckId}/coverage")]
    public async Task<ApiResponse<DeckLearningCoverageResponse>> GetDeckCoverage([FromRoute] string deckId)
    {
        var result = await _adminLearningService.GetDeckCoverageAsync(deckId);
        return ApiResponse<DeckLearningCoverageResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy dashboard analytics tổng quan của learning.
    /// </summary>
    [HttpGet("overview")]
    public async Task<ApiResponse<LearningAdminOverviewResponse>> GetOverview()
    {
        var result = await _adminLearningService.GetOverviewAsync();
        return ApiResponse<LearningAdminOverviewResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy analytics học tập của một deck.
    /// </summary>
    [HttpGet("decks/{deckId}/analytics")]
    public async Task<ApiResponse<DeckLearningAnalyticsResponse>> GetDeckAnalytics([FromRoute] string deckId)
    {
        var result = await _adminLearningService.GetDeckAnalyticsAsync(deckId);
        return ApiResponse<DeckLearningAnalyticsResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy analytics học tập của một card.
    /// </summary>
    [HttpGet("cards/{cardId}/analytics")]
    public async Task<ApiResponse<CardLearningAnalyticsResponse>> GetCardAnalytics([FromRoute] string cardId)
    {
        var result = await _adminLearningService.GetCardAnalyticsAsync(cardId);
        return ApiResponse<CardLearningAnalyticsResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy summary tiến độ learning của một user.
    /// </summary>
    [HttpGet("users/{userId}/progress")]
    public async Task<ApiResponse<UserLearningProgressResponse>> GetUserProgress([FromRoute] string userId)
    {
        var result = await _adminLearningService.GetUserProgressAsync(userId);
        return ApiResponse<UserLearningProgressResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Preview nội dung câu hỏi mà user frontend sẽ thấy cho một card.
    /// </summary>
    [HttpGet("cards/{cardId}/preview")]
    public async Task<ApiResponse<LearningPreviewResponse>> PreviewCard([FromRoute] string cardId, [FromQuery] LearningPreviewQuery query)
    {
        var result = await _adminLearningService.PreviewCardAsync(cardId, query);
        return ApiResponse<LearningPreviewResponse>.SuccessResponse(result);
    }
}
