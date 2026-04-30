using Application.Common;
using Application.DTOs.ExamSessions;

namespace Application.IServices;

public interface IExamSessionService
{
    Task<SessionStartResponse> StartSessionAsync(StartSessionRequest request, string userId);
    Task<SessionStartResponse> GetSessionAsync(string sessionId, string userId);
    Task SaveAnswerAsync(string sessionId, SaveAnswerRequest request, string userId);
    Task<SubmitSessionResponse> SubmitSessionAsync(string sessionId, string userId);
    Task<SessionResultResponse> GetSessionResultAsync(string sessionId, string userId);
    Task<(List<SessionListItemResponse> Items, MetaData Meta)> GetSessionHistoryAsync(SessionHistoryQuery query, string userId);
}
