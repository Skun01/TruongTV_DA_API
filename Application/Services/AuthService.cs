using Application.DTOs.Auth;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailSenderService _emailService;
    private readonly IEmailTemplateService _emailTemplate;
    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<JwtSettings> jwtSettings,
        IEmailSenderService emailService, IEmailTemplateService emailTemplate)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
        _emailTemplate = emailTemplate;
    }

    public async Task<AuthDTO> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        if(user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ApplicationException(MessageConstants.AuthMessage.INVALID_LOGIN);
        
        var accessToken = _tokenService.GenerateAccessToken(user);

        var userRefreshToken = new RefreshToken()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(userRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthDTO()
        {
            AccessToken = accessToken,
            RefreshToken = userRefreshToken.Token
        };
    }

    public async Task<bool> LogoutAsync(string? refreshToken)
    {
        if(refreshToken == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
            
        var userRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if(userRefreshToken != null)
        {
            userRefreshToken.Revoked = true;
            await _unitOfWork.SaveChangesAsync();
        }

        return true;
    }

    public async Task<AuthDTO> RefreshTokenAsync(string? refreshToken)
    {
        if(refreshToken == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
            
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if(storedToken == null || storedToken.Revoked)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.UNAUTHORIZED);
        
        if(storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException(MessageConstants.AuthMessage.TOKEN_EXPIRED);

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
        if(user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        storedToken.Revoked = true;
    
        // tạo token mới
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
            Revoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthDTO()
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        if(await _unitOfWork.Users.IsEmailExist(request.Email))
            throw new ApplicationException(MessageConstants.AuthMessage.EMAIL_EXIST);

        User newUser = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenHash = HashToken(request.Token);
        var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(tokenHash);

        if(user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            throw new ApplicationException(MessageConstants.AuthMessage.TOKEN_EXPIRED);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SendResetPasswordEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if(user != null)
        {
            var rawToken = _tokenService.GenerateRandomToken();
            user.PasswordResetToken = HashToken(rawToken);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ResetPasswordTokenExpireMinutes);
            
            _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            string emailTemplate = _emailTemplate.GetPasswordResetTemplate(user.Username, rawToken, 
                _jwtSettings.ResetPasswordTokenExpireMinutes);

            await _emailService.SendEmailAsync(
                user.Email,
                EmailSubjectConstants.RESET_PASSWORD,
                emailTemplate
            );
        }

        return true;
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
