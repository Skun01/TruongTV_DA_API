using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShadowingAttemptRepository : Repository<ShadowingAttempt>, IShadowingAttemptRepository
{
    public ShadowingAttemptRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<ShadowingAttempt> Items, int Total)> SearchByUserAsync(
        string userId,
        string? topicId,
        string? sentenceId,
        int page,
        int pageSize)
    {
        var attemptQuery = _context.ShadowingAttempts
            .AsNoTracking()
            .Include(x => x.Topic)
            .Include(x => x.Sentence)
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(topicId))
            attemptQuery = attemptQuery.Where(x => x.TopicId == topicId);

        if (!string.IsNullOrWhiteSpace(sentenceId))
            attemptQuery = attemptQuery.Where(x => x.SentenceId == sentenceId);

        var total = await attemptQuery.CountAsync();
        var items = await attemptQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<ShadowingAttempt>> GetByUserAndSentenceAsync(string userId, string sentenceId)
    {
        return await _context.ShadowingAttempts
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SentenceId == sentenceId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByTopicAsync(string topicId)
    {
        return await _context.ShadowingAttempts.CountAsync(x => x.TopicId == topicId);
    }

    public async Task<int> CountDistinctUsersByTopicAsync(string topicId)
    {
        return await _context.ShadowingAttempts
            .Where(x => x.TopicId == topicId)
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync();
    }

    public async Task<double?> GetAveragePronScoreByTopicAsync(string topicId)
    {
        return await _context.ShadowingAttempts
            .Where(x => x.TopicId == topicId && x.PronScore.HasValue)
            .Select(x => x.PronScore)
            .AverageAsync();
    }

    public async Task<DateTime?> GetLatestAttemptAtByTopicAsync(string topicId)
    {
        return await _context.ShadowingAttempts
            .Where(x => x.TopicId == topicId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (DateTime?)x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
