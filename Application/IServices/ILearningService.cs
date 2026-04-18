using Application.DTOs.Learning;

namespace Application.IServices;

public interface ILearningService
{
    Task<StudySessionResponse> CreateSessionAsync(CreateStudySessionRequest request, string userId);
    Task<StudySessionResponse> GetSessionAsync(string sessionId, string userId);
    Task<StudyQuestionResponse?> GetNextQuestionAsync(string sessionId, string userId);
    Task<SubmitStudyAnswerResponse> SubmitAnswerAsync(string sessionId, SubmitStudyAnswerRequest request, string userId);
    Task<TodayReviewSummaryResponse> GetTodayReviewAsync(TodayReviewQuery query, string userId);
}
