using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IExamSessionAiAnalysisRepository : IRepository<ExamSessionAiAnalysis>
{
    Task<ExamSessionAiAnalysis?> GetLatestCompletedAsync(string examSessionId, string promptVersion, string inputHash);
    Task<int> CountByUserAndTriggerTypeSinceAsync(string userId, ExamSessionAiAnalysisTriggerType triggerType, DateTime fromUtc);
}
