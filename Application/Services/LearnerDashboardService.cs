using Application.DTOs.Dashboard.Learner;
using Application.DTOs.Learning;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class LearnerDashboardService : ILearnerDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public LearnerDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LearnerStreakResponse> GetStreakAsync(string userId)
    {
        var completedSessions = await _unitOfWork.StudySessions.GetCompletedByUserOrderedAsync(userId);
        if (completedSessions.Count == 0)
            return new LearnerStreakResponse { CurrentStreak = 0, LongestStreak = 0, LastStudyDate = null };

        var studyDates = completedSessions
            .Where(s => s.CompletedAt.HasValue)
            .Select(s => DateOnly.FromDateTime(s.CompletedAt!.Value))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var lastStudyDate = studyDates[0];
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Tính current streak: đếm ngày liên tiếp ngược từ lastStudyDate
        var currentStreak = 0;
        var checkDate = lastStudyDate == today ? today.AddDays(-1) : lastStudyDate;
        while (studyDates.Contains(checkDate))
        {
            currentStreak++;
            checkDate = checkDate.AddDays(-1);
        }
        if (lastStudyDate == today || lastStudyDate == today.AddDays(-1))
            currentStreak++;

        // Tính longest streak: quét từ cũ đến mới
        var longestStreak = 0;
        var tempStreak = 0;
        var prevDate = (DateOnly?)null;
        foreach (var date in studyDates.OrderBy(d => d))
        {
            if (prevDate == null || date == prevDate.Value.AddDays(1))
                tempStreak++;
            else
                tempStreak = 1;

            if (tempStreak > longestStreak)
                longestStreak = tempStreak;

            prevDate = date;
        }

        return new LearnerStreakResponse
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            LastStudyDate = lastStudyDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
        };
    }

    public async Task<UpcomingReviewsResponse> GetUpcomingReviewsAsync(string userId, int days)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var todayEndUtc = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var tomorrowStartUtc = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var tomorrowEndUtc = today.AddDays(2).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var weekEndUtc = today.AddDays(days).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var dueByUserTask = _unitOfWork.UserCardProgresses.GetDueByUserAsync(userId, now);
        var upcomingTask = _unitOfWork.UserCardProgresses.GetUpcomingByUserAsync(userId, todayEndUtc, weekEndUtc);

        await Task.WhenAll(dueByUserTask, upcomingTask);

        var dueNow = await dueByUserTask;
        var upcoming = await upcomingTask;

        var dueByDay = upcoming
            .GroupBy(p => DateOnly.FromDateTime(p.NextReviewAt))
            .OrderBy(g => g.Key)
            .Select(g => new DailyReviewCount { Date = g.Key, Count = g.Count() })
            .ToList();

        return new UpcomingReviewsResponse
        {
            DueToday = dueNow.Count,
            DueTomorrow = upcoming.Count(p => p.NextReviewAt >= tomorrowStartUtc && p.NextReviewAt < tomorrowEndUtc),
            DueThisWeek = dueNow.Count + upcoming.Count,
            DueByDay = dueByDay,
        };
    }

    public async Task<DeckProgressResponse> GetDeckProgressAsync(string userId)
    {
        var decks = await _unitOfWork.Decks.GetAllReadableDecksWithFoldersAsync(userId);
        if (decks.Count == 0)
            return new DeckProgressResponse { Decks = new List<DeckProgressItem>() };

        var allCardIds = decks
            .SelectMany(d => d.Folders)
            .SelectMany(f => f.FolderCards)
            .Select(fc => fc.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var progresses = allCardIds.Count == 0
            ? new List<UserCardProgress>()
            : await _unitOfWork.UserCardProgresses.GetByCardIdsAsync(allCardIds);

        var now = DateTime.UtcNow;
        var deckItems = decks
            .Select(deck =>
            {
                var deckCardIds = deck.Folders
                    .SelectMany(f => f.FolderCards)
                    .Select(fc => fc.CardId)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var deckProgresses = progresses
                    .Where(p => deckCardIds.Contains(p.CardId, StringComparer.Ordinal))
                    .ToList();

                var totalCards = deckCardIds.Count;
                var masteredCards = deckProgresses
                    .Count(p => p.IsMastered || p.SrsLevel == SrsLevel.level_12);
                var dueCards = deckProgresses
                    .Count(p => !p.IsMastered && p.SrsLevel != SrsLevel.level_12 && p.NextReviewAt <= now);
                var learningCards = deckProgresses
                    .Count(p => !p.IsMastered && p.SrsLevel != SrsLevel.level_12 && p.NextReviewAt > now);

                return new DeckProgressItem
                {
                    DeckId = deck.Id,
                    DeckTitle = deck.Title,
                    TotalCards = totalCards,
                    MasteredCards = masteredCards,
                    DueCards = dueCards,
                    LearningCards = learningCards,
                    CompletionPercent = totalCards == 0 ? 0 : Math.Round((double)masteredCards / totalCards * 100, 2),
                };
            })
            .Where(x => x.TotalCards > 0)
            .OrderByDescending(x => x.DueCards)
            .ToList();

        return new DeckProgressResponse { Decks = deckItems };
    }

    public async Task<LearnerDashboardSummaryResponse> GetSummaryAsync(string userId)
    {
        var streakTask = GetStreakAsync(userId);
        var upcomingTask = GetUpcomingReviewsAsync(userId, 7);
        var deckProgressTask = GetDeckProgressAsync(userId);
        var sessionsTask = _unitOfWork.StudySessions.GetRecentByUserAsync(userId, 5);

        await Task.WhenAll(streakTask, upcomingTask, deckProgressTask, sessionsTask);

        var streak = await streakTask;
        var upcoming = await upcomingTask;
        var deckProgress = await deckProgressTask;
        var sessions = await sessionsTask;

        var recentSessions = sessions
            .Where(s => s.CompletedAt.HasValue)
            .OrderByDescending(s => s.CompletedAt)
            .Take(5)
            .Select(s =>
            {
                var attempts = s.CorrectCount + s.IncorrectCount;
                return new RecentSessionSummary
                {
                    Id = s.Id,
                    Mode = s.Mode.ToString(),
                    CorrectCount = s.CorrectCount,
                    IncorrectCount = s.IncorrectCount,
                    Accuracy = attempts == 0 ? 0 : Math.Round((double)s.CorrectCount / attempts * 100, 2),
                    CompletedAt = s.CompletedAt,
                };
            })
            .ToList();

        return new LearnerDashboardSummaryResponse
        {
            Streak = streak,
            TodayReview = new Application.DTOs.Learning.TodayReviewSummaryResponse
            {
                DueCount = upcoming.DueToday,
                TotalCards = upcoming.DueToday,
            },
            UpcomingReviews = new UpcomingReviewsSummary
            {
                DueToday = upcoming.DueToday,
                DueTomorrow = upcoming.DueTomorrow,
                DueThisWeek = upcoming.DueThisWeek,
            },
            DeckProgress = deckProgress.Decks.Take(5).ToList(),
            RecentSessions = recentSessions,
        };
    }

    public async Task<ExamHistoryResponse> GetExamHistoryAsync(ExamHistoryQuery query, string userId)
    {
        var limit = Math.Clamp(query.Limit, 1, 100);
        var sessionsTask = _unitOfWork.ExamSessions.GetRecentByUserAsync(userId, limit);
        var statsTask = _unitOfWork.ExamSessions.GetHistoryStatsByUserAsync(userId);

        await Task.WhenAll(sessionsTask, statsTask);

        var sessions = await sessionsTask;
        var stats = await statsTask;

        return new ExamHistoryResponse
        {
            Items = sessions.Select(x => x.ToExamHistoryItem()).ToList(),
            Stats = stats.ToExamHistoryStats(),
        };
    }
}
