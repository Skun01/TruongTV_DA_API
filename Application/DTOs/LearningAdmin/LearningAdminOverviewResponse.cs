namespace Application.DTOs.LearningAdmin;

public class LearningAdminOverviewResponse
{
    public int ActiveUsersToday { get; set; }
    public int SessionsToday { get; set; }
    public int CompletedSessionsToday { get; set; }
    public int SubmissionsToday { get; set; }
    public int DueCardsNow { get; set; }
    public double AverageAccuracy { get; set; }
}
