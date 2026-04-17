using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/decks")]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
public class AdminDecksController : BaseController
{
    private readonly IDeckAdminService _deckAdminService;

    public AdminDecksController(IDeckAdminService deckAdminService)
    {
        _deckAdminService = deckAdminService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<AdminDeckListItemResponse>>> Search([FromQuery] AdminDeckListQuery query)
    {
        var (items, meta) = await _deckAdminService.SearchAsync(query);
        return ApiResponse<List<AdminDeckListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("{deckId}")]
    public async Task<ApiResponse<AdminDeckDetailResponse>> GetDetail([FromRoute] string deckId)
    {
        var result = await _deckAdminService.GetDetailAsync(deckId);
        return ApiResponse<AdminDeckDetailResponse>.SuccessResponse(result);
    }

    [HttpPost]
    public async Task<ApiResponse<AdminDeckDetailResponse>> Create([FromBody] CreateAdminDeckRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _deckAdminService.CreateAsync(request, currentUserId);
        return ApiResponse<AdminDeckDetailResponse>.SuccessResponse(result);
    }

    [HttpPatch("{deckId}")]
    public async Task<ApiResponse<AdminDeckDetailResponse>> Update([FromRoute] string deckId, [FromBody] UpdateAdminDeckRequest request)
    {
        var result = await _deckAdminService.UpdateAsync(deckId, request);
        return ApiResponse<AdminDeckDetailResponse>.SuccessResponse(result);
    }

    [HttpDelete("{deckId}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string deckId)
    {
        var result = await _deckAdminService.DeleteAsync(deckId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    [HttpPost("{deckId}/publish")]
    public async Task<ApiResponse<AdminDeckDetailResponse>> Publish([FromRoute] string deckId)
    {
        var result = await _deckAdminService.PublishAsync(deckId);
        return ApiResponse<AdminDeckDetailResponse>.SuccessResponse(result);
    }

    [HttpPost("{deckId}/archive")]
    public async Task<ApiResponse<AdminDeckDetailResponse>> Archive([FromRoute] string deckId)
    {
        var result = await _deckAdminService.ArchiveAsync(deckId);
        return ApiResponse<AdminDeckDetailResponse>.SuccessResponse(result);
    }

    [HttpPost("{deckId}/unpublish")]
    public async Task<ApiResponse<AdminDeckDetailResponse>> Unpublish([FromRoute] string deckId)
    {
        var result = await _deckAdminService.UnpublishAsync(deckId);
        return ApiResponse<AdminDeckDetailResponse>.SuccessResponse(result);
    }

    [HttpPost("{deckId}/folders")]
    public async Task<ApiResponse<DeckFolderResponse>> CreateFolder([FromRoute] string deckId, [FromBody] CreateDeckFolderRequest request)
    {
        var result = await _deckAdminService.CreateFolderAsync(deckId, request);
        return ApiResponse<DeckFolderResponse>.SuccessResponse(result);
    }

    [HttpPut("{deckId}/folders/order")]
    public async Task<ApiResponse<List<DeckFolderResponse>>> ReorderFolders([FromRoute] string deckId, [FromBody] ReorderDeckFoldersRequest request)
    {
        var result = await _deckAdminService.ReorderDeckFoldersAsync(deckId, request);
        return ApiResponse<List<DeckFolderResponse>>.SuccessResponse(result);
    }
}
