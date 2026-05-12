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

    /// <summary>
    /// Tìm kiếm danh sách loại bộ thẻ (admin)
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<AdminDeckTypeResponse>>> Search([FromQuery] AdminDeckTypeListQuery query)
    {
        var (items, meta) = await _deckTypeAdminService.SearchAsync(query);
        return ApiResponse<List<AdminDeckTypeResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết loại bộ thẻ (admin)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<AdminDeckTypeResponse>> GetDetail([FromRoute] string id)
    {
        var result = await _deckTypeAdminService.GetDetailAsync(id);
        return ApiResponse<AdminDeckTypeResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tạo loại bộ thẻ mới (admin)
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<AdminDeckTypeResponse>> Create([FromBody] CreateDeckTypeRequest request)
    {
        var result = await _deckTypeAdminService.CreateAsync(request);
        return ApiResponse<AdminDeckTypeResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật loại bộ thẻ (admin)
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ApiResponse<AdminDeckTypeResponse>> Update([FromRoute] string id, [FromBody] UpdateDeckTypeRequest request)
    {
        var result = await _deckTypeAdminService.UpdateAsync(id, request);
        return ApiResponse<AdminDeckTypeResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa loại bộ thẻ (admin)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string id)
    {
        var result = await _deckTypeAdminService.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(result);
    }
}
