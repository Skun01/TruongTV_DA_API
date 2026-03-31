using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string Username { set; get; } = string.Empty;
    public string Email { set; get; } = string.Empty;
    public string? AvatarUrl { set; get; }
    public UserRole Role { set; get; } = UserRole.User;
    public string PasswordHash { set; get; } = string.Empty;
    public bool IsVerified { set; get; }
    public string? PasswordResetToken { set; get; }
    public DateTime? PasswordResetTokenExpiry { set; get; }
}
