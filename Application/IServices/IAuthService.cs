using Application.DTOs.Auth;

namespace Application.IServices;

public interface IAuthService
{
    public Task<AuthDTO> LoginAsync(LoginRequest request);
    public Task<AuthDTO> RegisterAsync(RegisterRequest request);
    public Task<AuthUserDTO> GetCurrentUserAsync(string userId);
    public Task<AuthUserDTO> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    public Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    public Task<bool> SendResetPasswordEmailAsync(string email);
    public Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    public Task<AuthDTO> RefreshTokenAsync(string? refreshToken);
    public Task<bool> LogoutAsync(string? refreshToken);
}
