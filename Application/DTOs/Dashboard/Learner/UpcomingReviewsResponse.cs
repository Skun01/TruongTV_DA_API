namespace Application.DTOs.Dashboard.Learner;

public class UpcomingReviewsResponse
{
    public int DueToday { get; set; }
    public int DueTomorrow { get; set; }
    public int DueThisWeek { get; set; }
    public List<DailyReviewCount> DueByDay { get; set; } = new();
}

public class DailyReviewCount
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}