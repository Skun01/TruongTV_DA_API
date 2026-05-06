using Application.DTOs.ExamSessions;

namespace Application.IServices;

public interface IExamSessionAiAnalysisService
{
    Task<JlptAiAnalysisResponse> GetAsync(string sessionId, string userId);
    Task<JlptAiAnalysisResponse> RegenerateAsync(string sessionId, RegenerateJlptAiAnalysisRequest request, string userId);
}
