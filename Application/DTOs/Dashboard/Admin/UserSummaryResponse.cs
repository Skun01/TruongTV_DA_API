namespace Application.DTOs.Dashboard.Admin;

public class UserSummaryResponse
{
    public int TotalUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int ActiveUsersToday { get; set; }
}