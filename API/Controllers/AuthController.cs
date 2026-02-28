using API.Extensions;
using Application.Common;
using Application.DTOs.Auth;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("auth")]
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
    public async Task<ApiResponse<bool>> Register([FromBody] RegisterRequest request)
    {
        var result = await HandleException(_authService.RegisterAsync(request));

        return result;
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ApiResponse<AuthDTO>> Login([FromBody] LoginRequest request)
    {
        var result = await HandleException(_authService.LoginAsync(request));
        if(result.Data != null)
            Response.SetRefreshTokenCookieExtension(result.Data.RefreshToken);

        return result;
    }


    /// <summary>
    /// Lấy token mới khi token cũ hết hạn
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh")]
    public async Task<ApiResponse<AuthDTO>> RefreshToken()
    {
        var refreshToken = Request.Cookies[CookieConstants.RefreshToken];
        var result = await HandleException(_authService.RefreshTokenAsync(refreshToken));
        if(result.Data != null)
            Response.SetRefreshTokenCookieExtension(result.Data.RefreshToken);

        return result;
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout")]
    public async Task<ApiResponse<bool>> Logout()
    {
        var refreshToken = Request.Cookies[CookieConstants.RefreshToken];
        var result = await HandleException(_authService.LogoutAsync(refreshToken));
        Response.DeleteRefreshTokenCookieExtension();

        return result;
    }

    /// <summary>
    /// Gửi mail yêu cầu reset password
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("forgot-password")]
    public async Task<ApiResponse<bool>> SendResetEmail([FromQuery] string email)
    {
        var result = await HandleException(_authService.SendResetPasswordEmailAsync(email));

        return result;
    }

    /// <summary>
    /// Xử lý yêu cầu reset password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("reset-password")]
    public async Task<ApiResponse<bool>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await HandleException(_authService.ResetPasswordAsync(request));

        return result;
    }
}
