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
    /// Preview nội dung câu hỏi mà user frontend sẽ thấy cho một card.
    /// </summary>
    [HttpGet("cards/{cardId}/preview")]
    public async Task<ApiResponse<LearningPreviewResponse>> PreviewCard([FromRoute] string cardId, [FromQuery] LearningPreviewQuery query)
    {
        var result = await _adminLearningService.PreviewCardAsync(cardId, query);
        return ApiResponse<LearningPreviewResponse>.SuccessResponse(result);
    }
}
