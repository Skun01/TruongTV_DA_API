namespace Domain.Entities;

public class UserSettings : BaseEntity
{
    public string UserId { set; get; } = null!;
    public int DailyGoal { set; get; } = 10;
    public int BatchSize { set; get; } = 5;
    public int CurrentStreak { set; get; } = 0;
    public int LongestStreak { set; get; } = 0;
    public DateTime? LastStudyDate { set; get; }

    public virtual User User { set; get; } = null!;
}
