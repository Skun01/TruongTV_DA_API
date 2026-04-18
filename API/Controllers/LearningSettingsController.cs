using Application.Common;
using Application.DTOs.Learning;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/learning/settings")]
public class LearningSettingsController : BaseController
{
    private readonly IUserLearningSettingsService _userLearningSettingsService;

    public LearningSettingsController(IUserLearningSettingsService userLearningSettingsService)
    {
        _userLearningSettingsService = userLearningSettingsService;
    }

    /// <summary>
    /// Lấy default learning settings hiện tại của user.
    /// Các settings này sẽ được dùng làm mặc định khi tạo study session mới.
    /// </summary>
    [HttpGet("me")]
    public async Task<ApiResponse<StudySessionSettingsResponse>> GetMine()
    {
        var result = await _userLearningSettingsService.GetAsync(GetCurrentUserId());
        return ApiResponse<StudySessionSettingsResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tạo mới hoặc cập nhật default learning settings của user.
    /// </summary>
    [HttpPut("me")]
    public async Task<ApiResponse<StudySessionSettingsResponse>> UpsertMine([FromBody] StudySessionSettingsRequest request)
    {
        var result = await _userLearningSettingsService.UpsertAsync(GetCurrentUserId(), request);
        return ApiResponse<StudySessionSettingsResponse>.SuccessResponse(result);
    }
}
