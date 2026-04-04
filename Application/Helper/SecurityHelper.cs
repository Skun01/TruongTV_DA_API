using System.Security.Cryptography;
using System.Text;

namespace Application.Helper;

public static class SecurityHelper
{
    public static string HashSha256Token(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
