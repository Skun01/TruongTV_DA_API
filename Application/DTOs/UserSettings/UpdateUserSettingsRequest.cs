namespace Application.DTOs.UserSettings;

public class UpdateUserSettingsRequest
{
    public int? DailyGoal { set; get; }
    public int? BatchSize { set; get; }
}
