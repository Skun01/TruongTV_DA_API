using Application.DTOs.Dashboard.Learner;
using Application.DTOs.Learning;

namespace Application.IServices;

public interface ILearnerDashboardService
{
    Task<LearnerStreakResponse> GetStreakAsync(string userId);
    Task<UpcomingReviewsResponse> GetUpcomingReviewsAsync(string userId, int days);
    Task<DeckProgressResponse> GetDeckProgressAsync(string userId);
    Task<LearnerDashboardSummaryResponse> GetSummaryAsync(string userId);
    Task<ExamHistoryResponse> GetExamHistoryAsync(ExamHistoryQuery query, string userId);
}
