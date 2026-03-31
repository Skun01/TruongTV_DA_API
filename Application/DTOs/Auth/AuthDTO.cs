using System.Text.Json.Serialization;

namespace Application.DTOs.Auth;

public class AuthDTO
{
    public string AccessToken { set; get; } = string.Empty;
    public AuthUserDTO User { set; get; } = new();

    [JsonIgnore]
    public string RefreshToken { set; get; } = string.Empty;
}
