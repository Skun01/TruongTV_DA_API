using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/me/folders")]
public class MyDeckFoldersController : BaseController
{
    private readonly IDeckUserService _deckUserService;

    public MyDeckFoldersController(IDeckUserService deckUserService)
    {
        _deckUserService = deckUserService;
    }

    /// <summary>
    /// Cập nhật thư mục cá nhân
    /// </summary>
    [HttpPatch("{folderId}")]
    public async Task<ApiResponse<DeckFolderResponse>> UpdateFolder([FromRoute] string folderId, [FromBody] UpdateDeckFolderRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.UpdateFolderAsync(folderId, request, userId);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa thư mục cá nhân
    /// </summary>
    [HttpDelete("{folderId}")]
    public async Task<ApiResponse<bool>> DeleteFolder([FromRoute] string folderId)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.DeleteFolderAsync(folderId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Thêm thẻ vào thư mục cá nhân
    /// </summary>
    [HttpPost("{folderId}/cards")]
    public async Task<ApiResponse<DeckFolderResponse>> AddCard([FromRoute] string folderId, [FromBody] AddCardToFolderRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.AddCardToFolderAsync(folderId, request, userId);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa thẻ khỏi thư mục cá nhân
    /// </summary>
    [HttpDelete("{folderId}/cards/{cardId}")]
    public async Task<ApiResponse<bool>> RemoveCard([FromRoute] string folderId, [FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.RemoveCardFromFolderAsync(folderId, cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Sắp xếp lại thứ tự thẻ trong thư mục cá nhân
    /// </summary>
    [HttpPut("{folderId}/cards/order")]
    public async Task<ApiResponse<List<DeckFolderCardItemResponse>>> ReorderCards([FromRoute] string folderId, [FromBody] ReorderFolderCardsRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deckUserService.ReorderFolderCardsAsync(folderId, request, userId);
        return ApiResponse<List<DeckFolderCardItemResponse>>.SuccessResponse(result);
    }
}
