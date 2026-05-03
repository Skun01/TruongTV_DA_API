using Application.DTOs.Dashboard.Admin;

namespace Application.IServices;

public interface IAdminDashboardService
{
    Task<ContentSummaryResponse> GetContentSummaryAsync();
    Task<UserSummaryResponse> GetUserSummaryAsync();
}