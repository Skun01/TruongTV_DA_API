using Domain.Entities;

namespace Application.IRepositories;

public interface IShadowingAttemptRepository : IRepository<ShadowingAttempt>
{
    Task<(List<ShadowingAttempt> Items, int Total)> SearchByUserAsync(
        string userId,
        string? topicId,
        string? sentenceId,
        int page,
        int pageSize);

    Task<ShadowingAttempt?> GetByIdForUserAsync(string attemptId, string userId);
    Task<List<ShadowingAttempt>> GetByUserAndSentenceAsync(string userId, string sentenceId);
    Task<List<ShadowingAttempt>> GetByUserAndTopicAsync(string userId, string topicId);
    Task<int> CountByTopicAsync(string topicId);
    Task<int> CountDistinctUsersByTopicAsync(string topicId);
    Task<double?> GetAveragePronScoreByTopicAsync(string topicId);
    Task<DateTime?> GetLatestAttemptAtByTopicAsync(string topicId);
    Task<Dictionary<string, (int AttemptsCount, int DistinctUsersCount, double? AveragePronScore, DateTime? LatestAttemptAt)>> GetSentenceAnalyticsByTopicAsync(string topicId);
}
