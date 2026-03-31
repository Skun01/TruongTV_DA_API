namespace Application.DTOs.Auth;

public class AuthUserDTO
{
    public string Id { set; get; } = string.Empty;
    public string Email { set; get; } = string.Empty;
    public string DisplayName { set; get; } = string.Empty;
    public string? AvatarUrl { set; get; }
    public string Role { set; get; } = "user";
    public DateTime CreatedAt { set; get; }
}
