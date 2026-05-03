using Application.DTOs.Dashboard.Learner;

namespace Application.IServices;

public interface ILearnerDashboardService
{
    Task<LearnerStreakResponse> GetStreakAsync(string userId);
    Task<UpcomingReviewsResponse> GetUpcomingReviewsAsync(string userId, int days);
    Task<DeckProgressResponse> GetDeckProgressAsync(string userId);
}