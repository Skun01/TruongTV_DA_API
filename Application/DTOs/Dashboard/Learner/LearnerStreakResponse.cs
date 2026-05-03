namespace Application.DTOs.Dashboard.Learner;

public class LearnerStreakResponse
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastStudyDate { get; set; }
}