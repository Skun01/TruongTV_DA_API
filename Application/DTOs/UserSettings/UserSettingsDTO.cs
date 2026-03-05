namespace Application.DTOs.UserSettings;

public class UserSettingsDTO
{
    public int DailyGoal { set; get; }
    public int BatchSize { set; get; }
    public int CurrentStreak { set; get; }
    public int LongestStreak { set; get; }
    public DateTime? LastStudyDate { set; get; }
}
