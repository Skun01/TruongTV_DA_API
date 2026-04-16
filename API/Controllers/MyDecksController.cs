using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/me/decks")]
public class MyDecksController : BaseController
{
    private readonly IDeckUserService _deckUserService;

    public MyDecksController(IDeckUserService deckUserService)
    {
        _deckUserService = deckUserService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<DeckListItemResponse>>> GetMyDecks([FromQuery] MyDeckListQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _deckUserService.GetMyDecksAsync(query, userId);
        return ApiResponse<List<DeckListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpPost]
    public async Task<ApiResponse<DeckDetailResponse>> CreateMyDeck([FromBody] CreateMyDeckRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.CreateMyDeckAsync(request, userId);
        return ApiResponse<DeckDetailResponse>.SuccessResponse(result);
    }

    [HttpPatch("{deckId}")]
    public async Task<ApiResponse<DeckDetailResponse>> UpdateMyDeck([FromRoute] string deckId, [FromBody] UpdateMyDeckRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.UpdateMyDeckAsync(deckId, request, userId);
        return ApiResponse<DeckDetailResponse>.SuccessResponse(result);
    }

    [HttpDelete("{deckId}")]
    public async Task<ApiResponse<bool>> DeleteMyDeck([FromRoute] string deckId)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.DeleteMyDeckAsync(deckId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpGet("bookmarks")]
    public async Task<ApiResponse<List<DeckListItemResponse>>> GetBookmarkedDecks([FromQuery] BookmarkedDeckListQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _deckUserService.GetBookmarkedAsync(query, userId);
        return ApiResponse<List<DeckListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpPost("{deckId}/folders")]
    public async Task<ApiResponse<DeckFolderResponse>> CreateFolder([FromRoute] string deckId, [FromBody] CreateDeckFolderRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.CreateFolderAsync(deckId, request, userId);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    [HttpPut("{deckId}/folders/order")]
    public async Task<ApiResponse<List<DeckFolderResponse>>> ReorderFolders([FromRoute] string deckId, [FromBody] ReorderDeckFoldersRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.ReorderDeckFoldersAsync(deckId, request, userId);
        return ApiResponse<List<DeckFolderResponse>>.SuccessResponse(result);
    }
}
