using Domain.Entities;

namespace Application.IRepositories;

public interface IStudySessionRepository : IRepository<StudySession>
{
    Task<StudySession?> GetByIdForUserAsync(string sessionId, string userId);
    Task<List<StudySession>> GetRecentByUserAsync(string userId, int limit);
    Task<List<StudySession>> GetCreatedSinceAsync(DateTime fromUtc);
    Task<List<StudySession>> GetByDeckIdAsync(string deckId);
    Task<List<StudySession>> GetByCardIdAsync(string cardId);
    Task<List<StudySession>> GetCompletedByUserOrderedAsync(string userId);
}
