namespace Application.DTOs.Auth;

public class RegisterRequest
{
    public string? Username { set; get; }
    public string? DisplayName { set; get; }
    public string Email { set; get; } = string.Empty;
    public string Password { set; get; } = string.Empty;
}
