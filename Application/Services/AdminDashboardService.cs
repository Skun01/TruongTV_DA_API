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
        var vocabularyCount = await _unitOfWork.Cards.CountAsync(c => c.CardType == CardType.Vocab);
        var grammarCount = await _unitOfWork.Cards.CountAsync(c => c.CardType == CardType.Grammar);
        var kanjiCount = await _unitOfWork.Cards.CountAsync(c => c.CardType == CardType.Kanji);
        var deckCount = await _unitOfWork.Decks.CountAsync(d => d.Status == PublishStatus.Published);

        return new ContentSummaryResponse
        {
            VocabularyCount = vocabularyCount,
            GrammarCount = grammarCount,
            KanjiCount = kanjiCount,
            DeckCount = deckCount,
        };
    }

    public async Task<UserSummaryResponse> GetUserSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var todayStartUtc = now.Date;
        var weekStartUtc = todayStartUtc.AddDays(-(int)now.DayOfWeek);

        var totalUsers = await _unitOfWork.Users.CountAsync(u => u.Role == UserRole.User);
        var newUsersToday = await _unitOfWork.Users.CountAsync(u =>
            u.Role == UserRole.User && u.CreatedAt >= todayStartUtc);
        var newUsersThisWeek = await _unitOfWork.Users.CountAsync(u =>
            u.Role == UserRole.User && u.CreatedAt >= weekStartUtc);
        var sessionsCreatedToday = await _unitOfWork.StudySessions.GetCreatedSinceAsync(todayStartUtc);
        var activeUsersToday = sessionsCreatedToday
            .Select(s => s.UserId)
            .Distinct(StringComparer.Ordinal)
            .Count();

        return new UserSummaryResponse
        {
            TotalUsers = totalUsers,
            NewUsersToday = newUsersToday,
            NewUsersThisWeek = newUsersThisWeek,
            ActiveUsersToday = activeUsersToday,
        };
    }
}
