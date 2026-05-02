using Application.Common;
using Application.DTOs.Users;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API quản trị người dùng cho trang admin.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthPolicyConstants.AdminOnly)]
public class AdminUsersController : BaseController
{
    private readonly IUserAdminService _userAdminService;

    public AdminUsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    /// <summary>
    /// Tìm kiếm và lọc danh sách người dùng cho màn hình quản trị.
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<AdminUserListItemResponse>>> Search([FromQuery] AdminUserListQuery query)
    {
        var (items, meta) = await _userAdminService.SearchAsync(query);
        return ApiResponse<List<AdminUserListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một người dùng theo id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<AdminUserDetailResponse>> GetDetail([FromRoute] string id)
    {
        var result = await _userAdminService.GetDetailAsync(id);
        return ApiResponse<AdminUserDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật role của người dùng. API này không cho phép admin tự đổi role của chính mình.
    /// </summary>
    [HttpPatch("{id}/role")]
    public async Task<ApiResponse<AdminUserDetailResponse>> UpdateRole([FromRoute] string id, [FromBody] UpdateUserRoleRequest request)
    {
        var result = await _userAdminService.UpdateRoleAsync(id, request, GetCurrentUserId());
        return ApiResponse<AdminUserDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Khóa hoặc mở khóa tài khoản người dùng. Khi khóa, refresh token hiện có của user sẽ bị thu hồi.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ApiResponse<AdminUserDetailResponse>> UpdateStatus([FromRoute] string id, [FromBody] UpdateUserStatusRequest request)
    {
        var result = await _userAdminService.UpdateStatusAsync(id, request, GetCurrentUserId());
        return ApiResponse<AdminUserDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật trạng thái xác minh tài khoản của người dùng.
    /// </summary>
    [HttpPatch("{id}/verification")]
    public async Task<ApiResponse<AdminUserDetailResponse>> UpdateVerification([FromRoute] string id, [FromBody] UpdateUserVerificationRequest request)
    {
        var result = await _userAdminService.UpdateVerificationAsync(id, request);
        return ApiResponse<AdminUserDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Gửi email đặt lại mật khẩu cho người dùng theo id.
    /// </summary>
    [HttpPost("{id}/send-reset-password")]
    public async Task<ApiResponse<bool>> SendResetPasswordEmail([FromRoute] string id)
    {
        var result = await _userAdminService.SendResetPasswordEmailAsync(id);
        return ApiResponse<bool>.SuccessResponse(result);
    }
}
