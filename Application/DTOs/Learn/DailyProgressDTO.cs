namespace Application.DTOs.Learn;

public class DailyProgressDTO
{
    public int LearnedToday { set; get; }
    public int DailyGoal { set; get; }
    public bool IsGoalReached { set; get; }
    public int CurrentStreak { set; get; }
}
