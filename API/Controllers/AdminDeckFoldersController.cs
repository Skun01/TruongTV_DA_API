using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/folders")]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
public class AdminDeckFoldersController : BaseController
{
    private readonly IDeckAdminService _deckAdminService;

    public AdminDeckFoldersController(IDeckAdminService deckAdminService)
    {
        _deckAdminService = deckAdminService;
    }

    /// <summary>
    /// Cập nhật thư mục trong bộ thẻ (admin)
    /// </summary>
    [HttpPatch("{folderId}")]
    public async Task<ApiResponse<DeckFolderResponse>> Update([FromRoute] string folderId, [FromBody] UpdateDeckFolderRequest request)
    {
        var result = await _deckAdminService.UpdateFolderAsync(folderId, request);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa thư mục trong bộ thẻ (admin)
    /// </summary>
    [HttpDelete("{folderId}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string folderId)
    {
        var result = await _deckAdminService.DeleteFolderAsync(folderId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Thêm thẻ vào thư mục (admin)
    /// </summary>
    [HttpPost("{folderId}/cards")]
    public async Task<ApiResponse<DeckFolderResponse>> AddCard([FromRoute] string folderId, [FromBody] AddCardToFolderRequest request)
    {
        var result = await _deckAdminService.AddCardToFolderAsync(folderId, request);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa thẻ khỏi thư mục (admin)
    /// </summary>
    [HttpDelete("{folderId}/cards/{cardId}")]
    public async Task<ApiResponse<bool>> RemoveCard([FromRoute] string folderId, [FromRoute] string cardId)
    {
        var result = await _deckAdminService.RemoveCardFromFolderAsync(folderId, cardId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Sắp xếp lại thứ tự thẻ trong thư mục (admin)
    /// </summary>
    [HttpPut("{folderId}/cards/order")]
    public async Task<ApiResponse<List<DeckFolderCardItemResponse>>> ReorderCards([FromRoute] string folderId, [FromBody] ReorderFolderCardsRequest request)
    {
        var result = await _deckAdminService.ReorderFolderCardsAsync(folderId, request);
        return ApiResponse<List<DeckFolderCardItemResponse>>.SuccessResponse(result);
    }
}
