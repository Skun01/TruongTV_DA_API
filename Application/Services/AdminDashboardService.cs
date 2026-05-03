using Application.DTOs.Dashboard.Admin;
using Application.IRepositories;
using Application.IServices;
using Domain.Enums;

namespace Application.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ContentSummaryResponse> GetContentSummaryAsync()
    {
        var todayStartUtc = DateTime.UtcNow.Date;

        var vocabCountTask = _unitOfWork.Cards.CountAsync(c => c.CardType == CardType.Vocab);
        var grammarCountTask = _unitOfWork.Cards.CountAsync(c => c.CardType == CardType.Grammar);
        var kanjiCountTask = _unitOfWork.Cards.CountAsync(c => c.CardType == CardType.Kanji);
        var deckCountTask = _unitOfWork.Decks.CountAsync(d => d.Status == PublishStatus.Published);

        await Task.WhenAll(vocabCountTask, grammarCountTask, kanjiCountTask, deckCountTask);

        return new ContentSummaryResponse
        {
            VocabularyCount = await vocabCountTask,
            GrammarCount = await grammarCountTask,
            KanjiCount = await kanjiCountTask,
            DeckCount = await deckCountTask,
        };
    }

    public async Task<UserSummaryResponse> GetUserSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var todayStartUtc = now.Date;
        var weekStartUtc = todayStartUtc.AddDays(-(int)now.DayOfWeek);

        var totalUsersTask = _unitOfWork.Users.CountAsync(u => u.Role == UserRole.User);
        var newTodayTask = _unitOfWork.Users.CountAsync(u =>
            u.Role == UserRole.User && u.CreatedAt >= todayStartUtc);
        var newThisWeekTask = _unitOfWork.Users.CountAsync(u =>
            u.Role == UserRole.User && u.CreatedAt >= weekStartUtc);
        var activeTodayTask = _unitOfWork.StudySessions
            .GetCreatedSinceAsync(todayStartUtc)
            .ContinueWith(t => t.Result
                .Select(s => s.UserId)
                .Distinct(StringComparer.Ordinal)
                .Count());

        await Task.WhenAll(totalUsersTask, newTodayTask, newThisWeekTask, activeTodayTask);

        return new UserSummaryResponse
        {
            TotalUsers = await totalUsersTask,
            NewUsersToday = await newTodayTask,
            NewUsersThisWeek = await newThisWeekTask,
            ActiveUsersToday = await activeTodayTask,
        };
    }
}