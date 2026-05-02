using Application.DTOs.Users;
using Domain.Entities;

namespace Application.Mappings;

public static class UserMappings
{
    public static AdminUserListItemResponse ToAdminListItemResponse(this User user)
    {
        return new AdminUserListItemResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.Username,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString().ToLowerInvariant(),
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }

    public static AdminUserDetailResponse ToAdminDetailResponse(this User user)
    {
        return new AdminUserDetailResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.Username,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString().ToLowerInvariant(),
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}
