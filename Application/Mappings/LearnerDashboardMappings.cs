using Application.DTOs.Dashboard.Learner;
using Domain.Entities;

namespace Application.Mappings;

public static class LearnerDashboardMappings
{
    public static ExamHistoryItem ToExamHistoryItem(this ExamSession session)
    {
        var maxScore = session.SectionScores.Count != 0
            ? session.SectionScores.Sum(x => x.MaxScore)
            : session.Exam.Sections.Sum(x => x.MaxScore);

        return new ExamHistoryItem
        {
            ExamSessionId = session.Id,
            ExamId = session.ExamId,
            ExamTitle = session.Exam.Title,
            ExamLevel = session.Exam.Level.ToString(),
            StartedAt = session.StartedAt,
            SubmittedAt = session.SubmittedAt,
            TotalScore = session.TotalScore,
            MaxScore = maxScore,
            IsPassed = session.IsPassed,
            Accuracy = maxScore == 0 || !session.TotalScore.HasValue
                ? 0
                : Math.Round((double)session.TotalScore.Value / maxScore * 100, 2),
        };
    }

    public static ExamHistoryStats ToExamHistoryStats(
        this (int TotalExamsTaken, int TotalPassed, int TotalFailed, double AverageScore, double PassRate) stats)
    {
        return new ExamHistoryStats
        {
            TotalExamsTaken = stats.TotalExamsTaken,
            TotalPassed = stats.TotalPassed,
            TotalFailed = stats.TotalFailed,
            AverageScore = stats.AverageScore,
            PassRate = stats.PassRate,
        };
    }
}
