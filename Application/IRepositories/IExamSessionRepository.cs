using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IExamSessionRepository : IRepository<ExamSession>
{
    Task<ExamSession?> GetWithAnswersAsync(string id);
    Task<ExamSession?> GetFullDetailAsync(string id);
    Task<ExamSession?> GetActiveSessionByExamAsync(string userId, string examId);
    Task<List<ExamSession>> GetExpiredSessionsAsync();
    Task<(List<ExamSession> Items, int Total)> SearchByUserAsync(
        string userId,
        string? examId,
        ExamSessionStatus? status,
        int page,
        int pageSize);
    Task<List<ExamSession>> GetRecentByUserAsync(string userId, int limit);
    Task<(int TotalExamsTaken, int TotalPassed, int TotalFailed, double AverageScore, double PassRate)> GetHistoryStatsByUserAsync(string userId);
}
