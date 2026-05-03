using Application.Common;
using Application.DTOs.Dashboard.Admin;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
public class AdminDashboardController : BaseController
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Tổng hợp số lượng content: vocabulary, grammar, kanji, deck.
    /// </summary>
    [HttpGet("content/summary")]
    public async Task<ApiResponse<ContentSummaryResponse>> GetContentSummary()
    {
        var result = await _dashboardService.GetContentSummaryAsync();
        return ApiResponse<ContentSummaryResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tổng hợp số liệu user: tổng users, users mới hôm nay/tuần, active users hôm nay.
    /// </summary>
    [HttpGet("users/summary")]
    public async Task<ApiResponse<UserSummaryResponse>> GetUserSummary()
    {
        var result = await _dashboardService.GetUserSummaryAsync();
        return ApiResponse<UserSummaryResponse>.SuccessResponse(result);
    }
}