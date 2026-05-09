using Application.Common;
using Application.DTOs.CardNotes;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
public class CardNotesController : BaseController
{
    private readonly ICardNoteService _cardNoteService;

    public CardNotesController(ICardNoteService cardNoteService)
    {
        _cardNoteService = cardNoteService;
    }

    /// <summary>
    /// Lấy danh sách community notes của một card có phân trang.
    /// </summary>
    [HttpGet("api/cards/{cardId}/notes")]
    public async Task<ApiResponse<List<CardNoteResponse>>> GetCardCommunityNotes(
        [FromRoute] string cardId,
        [FromQuery] CardNoteListQuery query)
    {
        var userId = GetCurrentUserId();
        var (notes, meta) = await _cardNoteService.GetCardCommunityNotesAsync(cardId, userId, query);
        return ApiResponse<List<CardNoteResponse>>.SuccessResponse(notes, meta);
    }

    /// <summary>
    /// Tạo mới hoặc cập nhật ghi chú của chính người dùng cho card.
    /// </summary>
    [HttpPost("api/cards/{cardId}/notes")]
    public async Task<ApiResponse<CardNoteResponse>> UpsertMyCardNote([FromRoute] string cardId, [FromBody] UpsertCardNoteRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _cardNoteService.UpsertMyCardNoteAsync(cardId, userId, request);
        return ApiResponse<CardNoteResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa ghi chú của chính người dùng trên card.
    /// </summary>
    [HttpDelete("api/cards/{cardId}/notes/me")]
    public async Task<ApiResponse<bool>> DeleteMyCardNote([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _cardNoteService.DeleteMyCardNoteAsync(cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Bật hoặc tắt trạng thái thích cho một ghi chú.
    /// </summary>
    [HttpPost("api/notes/{noteId}/toggle-like")]
    public async Task<ApiResponse<ToggleCardNoteLikeResponse>> ToggleCardNoteLike([FromRoute] string noteId)
    {
        var userId = GetCurrentUserId();
        var result = await _cardNoteService.ToggleCardNoteLikeAsync(noteId, userId);
        return ApiResponse<ToggleCardNoteLikeResponse>.SuccessResponse(result);
    }
}
