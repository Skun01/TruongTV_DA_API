using Application.DTOs.Auth;
using Application.DTOs.Internal;
using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
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
    private readonly IFileUploadService _fileUploadService;
    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<JwtSettings> jwtSettings,
        IEmailSenderService emailService, IEmailTemplateService emailTemplate, IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
        _emailTemplate = emailTemplate;
        _fileUploadService = fileUploadService;
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
            RefreshToken = userRefreshToken.Token,
            User = MapToAuthUser(user),
        };
    }

    public async Task<AuthUserDTO> GetCurrentUserAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        return MapToAuthUser(user);
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
            RefreshToken = newRefreshToken.Token,
            User = MapToAuthUser(user),
        };
    }

    public async Task<AuthDTO> RegisterAsync(RegisterRequest request)
    {
        if(await _unitOfWork.Users.IsEmailExist(request.Email))
            throw new ApplicationException(MessageConstants.AuthMessage.EMAIL_EXIST);

        var displayName = request.DisplayName ?? request.Username;
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        User newUser = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Username = displayName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
        };

        await _unitOfWork.Users.AddAsync(newUser);

        var accessToken = _tokenService.GenerateAccessToken(newUser);
        var userRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = newUser.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(userRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthDTO
        {
            AccessToken = accessToken,
            RefreshToken = userRefreshToken.Token,
            User = MapToAuthUser(newUser),
        };
    }

    public async Task<AuthUserDTO> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        user.Username = request.DisplayName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToAuthUser(user);
    }

    public async Task<AuthUserDTO> UploadAvatarAsync(string userId, UploadAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if (request.Content == Stream.Null || request.SizeInBytes <= 0 || string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.ContentType))
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var oldAvatar = await _unitOfWork.MediaAssets.GetLatestByUserAndUsageAsync(userId, ResourceUsageType.Avatar);
        if (oldAvatar != null)
        {
            await _fileUploadService.DeleteAsync(oldAvatar.StorageKey, oldAvatar.FileType, cancellationToken);
            _unitOfWork.MediaAssets.DeleteAsync(oldAvatar);
        }

        var uploadResult = await _fileUploadService.UploadAsync(new FileUploadRequest
        {
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Content = request.Content,
            FileType = FileType.Image,
            UsageType = ResourceUsageType.Avatar,
        }, cancellationToken);

        var mediaAsset = new MediaAsset
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            FileUrl = uploadResult.FileUrl,
            StorageKey = uploadResult.StorageKey,
            OriginalFileName = request.FileName,
            ContentType = uploadResult.ContentType,
            SizeInBytes = request.SizeInBytes,
            FileType = uploadResult.FileType,
            UsageType = uploadResult.UsageType,
            StorageProvider = StorageProvider.Cloud,
        };

        await _unitOfWork.MediaAssets.AddAsync(mediaAsset);

        user.AvatarUrl = mediaAsset.FileUrl;
        _unitOfWork.Users.UpdateAsync(user);

        await _unitOfWork.SaveChangesAsync();

        return MapToAuthUser(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ApplicationException(MessageConstants.AuthMessage.WRONG_CURRENT_PASSWORD);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _unitOfWork.Users.UpdateAsync(user);
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

    private static AuthUserDTO MapToAuthUser(User user)
    {
        return new AuthUserDTO
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.Username,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString().ToLowerInvariant(),
            CreatedAt = user.CreatedAt,
        };
    }
}
