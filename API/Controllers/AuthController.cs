using API.Extensions;
using Application.Common;
using Application.DTOs.Auth;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng ký 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<ApiResponse<AuthDTO>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        Response.SetRefreshTokenCookieExtension(result.RefreshToken);
        return ApiResponse<AuthDTO>.SuccessResponse(result);
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ApiResponse<AuthDTO>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        Response.SetRefreshTokenCookieExtension(result.RefreshToken);
        return ApiResponse<AuthDTO>.SuccessResponse(result);
    }


    /// <summary>
    /// Lấy token mới khi token cũ hết hạn
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh-token")]
    public async Task<ApiResponse<AuthDTO>> RefreshToken()
    {
        var refreshToken = Request.Cookies[CookieConstants.RefreshToken];
        var result = await _authService.RefreshTokenAsync(refreshToken);
        Response.SetRefreshTokenCookieExtension(result.RefreshToken);
        return ApiResponse<AuthDTO>.SuccessResponse(result);
    }

    [HttpPost("refresh")]
    public async Task<ApiResponse<AuthDTO>> RefreshTokenAlias()
    {
        return await RefreshToken();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ApiResponse<AuthUserDTO>> Me()
    {
        var userId = GetCurrentUserId();
        var result = await _authService.GetCurrentUserAsync(userId);
        return ApiResponse<AuthUserDTO>.SuccessResponse(result);
    }

    [Authorize]
    [HttpPatch("me/profile")]
    public async Task<ApiResponse<AuthUserDTO>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.UpdateProfileAsync(userId, request);
        return ApiResponse<AuthUserDTO>.SuccessResponse(result);
    }

    [Authorize]
    [HttpPatch("change-password")]
    public async Task<ApiResponse<bool>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.ChangePasswordAsync(userId, request);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ApiResponse<bool>> Logout()
    {
        var refreshToken = Request.Cookies[CookieConstants.RefreshToken];
        var result = await _authService.LogoutAsync(refreshToken);
        Response.DeleteRefreshTokenCookieExtension();
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Gửi mail yêu cầu reset password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("forgot-password")]
    public async Task<ApiResponse<bool>> SendResetEmail([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.SendResetPasswordEmailAsync(request.Email);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Xử lý yêu cầu reset password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("reset-password")]
    public async Task<ApiResponse<bool>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return ApiResponse<bool>.SuccessResponse(result);
    }
}
