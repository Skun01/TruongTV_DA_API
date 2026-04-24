using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IShadowingTopicRepository : IRepository<ShadowingTopic>
{
    Task<(List<ShadowingTopic> Items, int Total)> SearchReadableAsync(
        string userId,
        string? query,
        JlptLevel? level,
        DeckVisibility? visibility,
        bool? officialOnly,
        int page,
        int pageSize);

    Task<(List<ShadowingTopic> Items, int Total)> SearchAdminAsync(
        string? query,
        JlptLevel? level,
        DeckVisibility? visibility,
        PublishStatus? status,
        bool? isOfficial,
        string? createdBy,
        int page,
        int pageSize);

    Task<ShadowingTopic?> GetReadableDetailByIdAsync(string topicId, string userId);
    Task<ShadowingTopic?> GetAdminDetailByIdAsync(string topicId);
}
