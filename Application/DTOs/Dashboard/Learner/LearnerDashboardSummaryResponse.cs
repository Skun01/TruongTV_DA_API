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
