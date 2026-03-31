namespace Application.DTOs.Auth;

public class ChangePasswordRequest
{
    public string CurrentPassword { set; get; } = string.Empty;
    public string NewPassword { set; get; } = string.Empty;
}