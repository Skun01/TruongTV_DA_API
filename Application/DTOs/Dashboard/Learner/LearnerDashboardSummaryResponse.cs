using Application.DTOs.Learning;

namespace Application.DTOs.Dashboard.Learner;

public class LearnerDashboardSummaryResponse
{
    public LearnerStreakResponse Streak { get; set; } = new();
    public TodayReviewSummaryResponse TodayReview { get; set; } = new();
    public UpcomingReviewsSummary UpcomingReviews { get; set; } = new();
    public List<DeckProgressItem> DeckProgress { get; set; } = new();
    public List<RecentSessionSummary> RecentSessions { get; set; } = new();
}

public class UpcomingReviewsSummary
{
    public int DueToday { get; set; }
    public int DueTomorrow { get; set; }
    public int DueThisWeek { get; set; }
}

public class RecentSessionSummary
{
    public string Id { get; set; } = string.Empty;
    public string? DeckTitle { get; set; }
    public string Mode { get; set; } = string.Empty;
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public double Accuracy { get; set; }
    public DateTime? CompletedAt { get; set; }
}