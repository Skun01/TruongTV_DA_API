namespace Application.DTOs.Auth;

public class UpdateProfileRequest
{
    public string DisplayName { set; get; } = string.Empty;
    public string? AvatarUrl { set; get; }
}