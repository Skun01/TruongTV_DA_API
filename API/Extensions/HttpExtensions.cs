using Domain.Constants;

namespace API.Extensions;

public static class HttpExtensions
{
    public static void SetRefreshTokenCookieExtension(this HttpResponse response, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true,
            SameSite = SameSiteMode.Lax
        };

        response.Cookies.Append(CookieConstants.RefreshToken, refreshToken, cookieOptions);
    }

    public static void DeleteRefreshTokenCookieExtension(this HttpResponse response)
    {
        response.Cookies.Delete(CookieConstants.RefreshToken);
    }
}
