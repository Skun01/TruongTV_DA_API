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

    [HttpGet("api/cards/{cardId}/notes")]
    public async Task<ApiResponse<List<CardNoteResponse>>> GetCardCommunityNotes(
        [FromRoute] string cardId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var (notes, meta) = await _cardNoteService.GetCardCommunityNotesAsync(cardId, userId, page, pageSize);
        return ApiResponse<List<CardNoteResponse>>.SuccessResponse(notes, meta);
    }

    [HttpPost("api/cards/{cardId}/notes")]
    public async Task<ApiResponse<CardNoteResponse>> UpsertMyCardNote([FromRoute] string cardId, [FromBody] UpsertCardNoteRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _cardNoteService.UpsertMyCardNoteAsync(cardId, userId, request);
        return ApiResponse<CardNoteResponse>.SuccessResponse(result);
    }

    [HttpDelete("api/cards/{cardId}/notes/me")]
    public async Task<ApiResponse<bool>> DeleteMyCardNote([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _cardNoteService.DeleteMyCardNoteAsync(cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpPost("api/notes/{noteId}/toggle-like")]
    public async Task<ApiResponse<ToggleCardNoteLikeResponse>> ToggleCardNoteLike([FromRoute] string noteId)
    {
        var userId = GetCurrentUserId();
        var result = await _cardNoteService.ToggleCardNoteLikeAsync(noteId, userId);
        return ApiResponse<ToggleCardNoteLikeResponse>.SuccessResponse(result);
    }
}
