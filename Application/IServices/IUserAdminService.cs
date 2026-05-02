using Application.Common;
using Application.DTOs.Users;

namespace Application.IServices;

public interface IUserAdminService
{
    Task<(List<AdminUserListItemResponse> Items, MetaData Meta)> SearchAsync(AdminUserListQuery query);
    Task<AdminUserDetailResponse> GetDetailAsync(string id);
    Task<AdminUserDetailResponse> UpdateRoleAsync(string id, UpdateUserRoleRequest request, string actorUserId);
    Task<AdminUserDetailResponse> UpdateStatusAsync(string id, UpdateUserStatusRequest request, string actorUserId);
    Task<AdminUserDetailResponse> UpdateVerificationAsync(string id, UpdateUserVerificationRequest request);
    Task<bool> SendResetPasswordEmailAsync(string id);
}
