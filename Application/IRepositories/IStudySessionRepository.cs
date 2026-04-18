using Domain.Entities;

namespace Application.IRepositories;

public interface IStudySessionRepository : IRepository<StudySession>
{
    Task<StudySession?> GetByIdForUserAsync(string sessionId, string userId);
}
