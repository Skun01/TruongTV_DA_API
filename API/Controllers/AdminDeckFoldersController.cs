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

    [HttpPatch("{folderId}")]
    public async Task<ApiResponse<DeckFolderResponse>> Update([FromRoute] string folderId, [FromBody] UpdateDeckFolderRequest request)
    {
        var result = await _deckAdminService.UpdateFolderAsync(folderId, request);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    [HttpDelete("{folderId}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string folderId)
    {
        var result = await _deckAdminService.DeleteFolderAsync(folderId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpPost("{folderId}/cards")]
    public async Task<ApiResponse<DeckFolderResponse>> AddCard([FromRoute] string folderId, [FromBody] AddCardToFolderRequest request)
    {
        var result = await _deckAdminService.AddCardToFolderAsync(folderId, request);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    [HttpDelete("{folderId}/cards/{cardId}")]
    public async Task<ApiResponse<bool>> RemoveCard([FromRoute] string folderId, [FromRoute] string cardId)
    {
        var result = await _deckAdminService.RemoveCardFromFolderAsync(folderId, cardId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpPut("{folderId}/cards/order")]
    public async Task<ApiResponse<List<DeckFolderCardItemResponse>>> ReorderCards([FromRoute] string folderId, [FromBody] ReorderFolderCardsRequest request)
    {
        var result = await _deckAdminService.ReorderFolderCardsAsync(folderId, request);
        return ApiResponse<List<DeckFolderCardItemResponse>>.SuccessResponse(result);
    }
}
