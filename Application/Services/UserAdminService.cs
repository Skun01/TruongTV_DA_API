using Application.Common;
using Application.DTOs.Users;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class UserAdminService : IUserAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public UserAdminService(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<(List<AdminUserListItemResponse> Items, MetaData Meta)> SearchAsync(AdminUserListQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var parsedRole = EnumParsingHelper.ParseNullable<UserRole>(query.Role);
        var (items, total) = await _unitOfWork.Users.SearchAsync(
            query.Q,
            parsedRole,
            query.IsActive,
            query.IsVerified,
            page,
            pageSize);

        return (
            items.Select(x => x.ToAdminListItemResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<AdminUserDetailResponse> GetDetailAsync(string id)
    {
        var user = await GetUserRequiredAsync(id);
        return user.ToAdminDetailResponse();
    }

    public async Task<AdminUserDetailResponse> UpdateRoleAsync(string id, UpdateUserRoleRequest request, string actorUserId)
    {
        if (string.Equals(actorUserId, id, StringComparison.Ordinal))
            throw new AppException(MessageConstants.UserMessage.CANNOT_CHANGE_OWN_ROLE, 400);

        var user = await GetUserRequiredAsync(id);
        user.Role = EnumParsingHelper.ParseRequired<UserRole>(request.Role);
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.RefreshTokens.RevokeByUserIdAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        return user.ToAdminDetailResponse();
    }

    public async Task<AdminUserDetailResponse> UpdateStatusAsync(string id, UpdateUserStatusRequest request, string actorUserId)
    {
        var isActive = request.IsActive ?? throw new AppException(MessageConstants.CommonMessage.INVALID, 400);
        if (string.Equals(actorUserId, id, StringComparison.Ordinal) && !isActive)
            throw new AppException(MessageConstants.UserMessage.CANNOT_DEACTIVATE_SELF, 400);

        var user = await GetUserRequiredAsync(id);
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.UpdateAsync(user);
        if (!isActive)
            await _unitOfWork.RefreshTokens.RevokeByUserIdAsync(user.Id);

        await _unitOfWork.SaveChangesAsync();

        return user.ToAdminDetailResponse();
    }

    public async Task<AdminUserDetailResponse> UpdateVerificationAsync(string id, UpdateUserVerificationRequest request)
    {
        var isVerified = request.IsVerified ?? throw new AppException(MessageConstants.CommonMessage.INVALID, 400);
        var user = await GetUserRequiredAsync(id);
        user.IsVerified = isVerified;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user.ToAdminDetailResponse();
    }

    public async Task<bool> SendResetPasswordEmailAsync(string id)
    {
        var user = await GetUserRequiredAsync(id);
        return await _authService.SendResetPasswordEmailAsync(user.Email);
    }

    private async Task<User> GetUserRequiredAsync(string id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new AppException(MessageConstants.UserMessage.NOT_FOUND, 404);
    }
}
