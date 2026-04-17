using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/deck-types")]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
public class AdminDeckTypesController : BaseController
{
    private readonly IDeckTypeAdminService _deckTypeAdminService;

    public AdminDeckTypesController(IDeckTypeAdminService deckTypeAdminService)
    {
        _deckTypeAdminService = deckTypeAdminService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<AdminDeckTypeResponse>>> Search([FromQuery] AdminDeckTypeListQuery query)
    {
        var (items, meta) = await _deckTypeAdminService.SearchAsync(query);
        return ApiResponse<List<AdminDeckTypeResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<AdminDeckTypeResponse>> GetDetail([FromRoute] string id)
    {
        var result = await _deckTypeAdminService.GetDetailAsync(id);
        return ApiResponse<AdminDeckTypeResponse>.SuccessResponse(result);
    }

    [HttpPost]
    public async Task<ApiResponse<AdminDeckTypeResponse>> Create([FromBody] CreateDeckTypeRequest request)
    {
        var result = await _deckTypeAdminService.CreateAsync(request);
        return ApiResponse<AdminDeckTypeResponse>.SuccessResponse(result);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<AdminDeckTypeResponse>> Update([FromRoute] string id, [FromBody] UpdateDeckTypeRequest request)
    {
        var result = await _deckTypeAdminService.UpdateAsync(id, request);
        return ApiResponse<AdminDeckTypeResponse>.SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string id)
    {
        var result = await _deckTypeAdminService.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(result);
    }
}
