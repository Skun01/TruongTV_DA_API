using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/decks")]
public class DecksController : BaseController
{
    private readonly IDeckUserService _deckUserService;

    public DecksController(IDeckUserService deckUserService)
    {
        _deckUserService = deckUserService;
    }

    /// <summary>
    /// Tìm kiếm danh sách bộ thẻ công khai
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponse<List<DeckListItemResponse>>> Search([FromQuery] DeckListQuery query)
    {
        var currentUserId = GetCurrentUserIdOrNull();
        var (items, meta) = await _deckUserService.SearchPublicAsync(query, currentUserId);
        return ApiResponse<List<DeckListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết bộ thẻ
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{deckId}")]
    public async Task<ApiResponse<DeckDetailResponse>> GetDetail([FromRoute] string deckId)
    {
        var currentUserId = GetCurrentUserIdOrNull();
        var result = await _deckUserService.GetDetailAsync(deckId, currentUserId);
        return ApiResponse<DeckDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Đánh dấu / bỏ đánh dấu bộ thẻ
    /// </summary>
    [Authorize]
    [HttpPost("{deckId}/bookmark")]
    public async Task<ApiResponse<DeckBookmarkResponse>> ToggleBookmark([FromRoute] string deckId, [FromBody] ToggleDeckBookmarkRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.ToggleBookmarkAsync(deckId, userId, request);
        return ApiResponse<DeckBookmarkResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Fork bộ thẻ về bộ sưu tập cá nhân
    /// </summary>
    [Authorize]
    [HttpPost("{deckId}/fork")]
    public async Task<ApiResponse<DeckDetailResponse>> Fork([FromRoute] string deckId)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.ForkAsync(deckId, userId);
        return ApiResponse<DeckDetailResponse>.SuccessResponse(result);
    }
}
