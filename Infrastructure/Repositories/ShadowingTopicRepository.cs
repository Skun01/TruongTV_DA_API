using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShadowingTopicRepository : Repository<ShadowingTopic>, IShadowingTopicRepository
{
    public ShadowingTopicRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<ShadowingTopic> Items, int Total)> SearchReadableAsync(
        string userId,
        string? query,
        JlptLevel? level,
        DeckVisibility? visibility,
        bool? officialOnly,
        int page,
        int pageSize)
    {
        var topicQuery = BuildTopicQuery()
            .Where(x => x.Status == PublishStatus.Published)
            .Where(x => x.Visibility == DeckVisibility.Public || x.CreatedBy == userId);

        topicQuery = ApplyCommonFilters(topicQuery, query, level, visibility, officialOnly);

        var total = await topicQuery.CountAsync();
        var items = await topicQuery
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<ShadowingTopic> Items, int Total)> SearchAdminAsync(
        string? query,
        JlptLevel? level,
        DeckVisibility? visibility,
        PublishStatus? status,
        bool? isOfficial,
        string? createdBy,
        int page,
        int pageSize)
    {
        var topicQuery = BuildTopicQuery();

        topicQuery = ApplyCommonFilters(topicQuery, query, level, visibility, null);

        if (status.HasValue)
            topicQuery = topicQuery.Where(x => x.Status == status.Value);

        if (isOfficial.HasValue)
            topicQuery = topicQuery.Where(x => x.IsOfficial == isOfficial.Value);

        if (!string.IsNullOrWhiteSpace(createdBy))
            topicQuery = topicQuery.Where(x => x.CreatedBy == createdBy);

        var total = await topicQuery.CountAsync();
        var items = await topicQuery
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<ShadowingTopic?> GetReadableDetailByIdAsync(string topicId, string userId)
    {
        return await BuildTopicDetailQuery()
            .Where(x => x.Id == topicId)
            .Where(x => x.Status == PublishStatus.Published)
            .Where(x => x.Visibility == DeckVisibility.Public || x.CreatedBy == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<ShadowingTopic?> GetAdminDetailByIdAsync(string topicId)
    {
        return await BuildTopicDetailQuery()
            .FirstOrDefaultAsync(x => x.Id == topicId);
    }

    private IQueryable<ShadowingTopic> BuildTopicQuery()
    {
        return _context.ShadowingTopics
            .AsNoTracking()
            .Include(x => x.Creator);
    }

    private IQueryable<ShadowingTopic> BuildTopicDetailQuery()
    {
        return _context.ShadowingTopics
            .AsNoTracking()
            .Include(x => x.Creator)
            .Include(x => x.TopicSentences)
                .ThenInclude(x => x.Sentence);
    }

    private static IQueryable<ShadowingTopic> ApplyCommonFilters(
        IQueryable<ShadowingTopic> query,
        string? search,
        JlptLevel? level,
        DeckVisibility? visibility,
        bool? officialOnly)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, pattern)
                || EF.Functions.ILike(x.Description, pattern));
        }

        if (level.HasValue)
            query = query.Where(x => x.Level == level.Value);

        if (visibility.HasValue)
            query = query.Where(x => x.Visibility == visibility.Value);

        if (officialOnly.HasValue && officialOnly.Value)
            query = query.Where(x => x.IsOfficial);

        return query;
    }
}
