using Application.Common;
using Application.DTOs.UserSettings;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("settings")]
[Authorize]
public class UserSettingsController : BaseController
{
    private readonly IUserSettingsService _service;
    public UserSettingsController(IUserSettingsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy cài đặt của người dùng
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResponse<UserSettingsDTO>> Get()
    {
        var result = await HandleException(_service.GetSettingsAsync(GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Cập nhật cài đặt (DailyGoal, BatchSize)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ApiResponse<bool>> Update([FromBody] UpdateUserSettingsRequest request)
    {
        var result = await HandleException(_service.UpdateSettingsAsync(request, GetCurrentUserId()));

        return result;
    }
}
