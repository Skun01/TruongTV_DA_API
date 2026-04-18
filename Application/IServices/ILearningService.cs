using Application.DTOs.Learning;

namespace Application.IServices;

public interface ILearningService
{
    Task<StudySessionResponse> CreateSessionAsync(CreateStudySessionRequest request, string userId);
    Task<StudySessionResponse> GetSessionAsync(string sessionId, string userId);
    Task<bool> DeleteSessionAsync(string sessionId, string userId);
    Task<List<StudySessionResponse>> GetHistoryAsync(StudyHistoryQuery query, string userId);
    Task<StudyQuestionResponse?> GetNextQuestionAsync(string sessionId, string userId);
    Task<SubmitStudyAnswerResponse> SubmitAnswerAsync(string sessionId, SubmitStudyAnswerRequest request, string userId);
    Task<CardProgressResponse> GetCardProgressAsync(string cardId, string userId);
    Task<StudySessionResultResponse> GetSessionResultAsync(string sessionId, string userId);
    Task<StudySessionResponse> RestartSessionAsync(string sessionId, string userId);
    Task<TodayReviewSummaryResponse> GetTodayReviewAsync(TodayReviewQuery query, string userId);
    Task<DueReviewCardsResponse> GetDueCardsAsync(DueReviewCardsQuery query, string userId);
}
