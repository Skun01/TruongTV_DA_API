using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Mappings;

public static class AuthMappings
{
    public static AuthUserDTO ToAuthUserDto(this User user)
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
