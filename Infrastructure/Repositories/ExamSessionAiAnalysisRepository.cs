using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExamSessionAiAnalysisRepository : Repository<ExamSessionAiAnalysis>, IExamSessionAiAnalysisRepository
{
    public ExamSessionAiAnalysisRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ExamSessionAiAnalysis?> GetLatestCompletedAsync(string examSessionId, string promptVersion, string inputHash)
    {
        return await _context.ExamSessionAiAnalyses
            .AsNoTracking()
            .Where(x => x.ExamSessionId == examSessionId)
            .Where(x => x.PromptVersion == promptVersion)
            .Where(x => x.InputHash == inputHash)
            .Where(x => x.Status == ExamSessionAiAnalysisStatus.Completed)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountByUserAndTriggerTypeSinceAsync(string userId, ExamSessionAiAnalysisTriggerType triggerType, DateTime fromUtc)
    {
        return await _context.ExamSessionAiAnalyses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Where(x => x.TriggerType == triggerType)
            .Where(x => x.CreatedAt >= fromUtc)
            .CountAsync();
    }
}
