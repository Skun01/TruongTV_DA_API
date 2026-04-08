using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SentenceRepository : Repository<Sentence>, ISentenceRepository
{
    public SentenceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Sentence> Items, int Total)> SearchAsync(
        string? query,
        JlptLevel? level,
        string? createdBy,
        bool? hasAudio,
        int page,
        int pageSize)
    {
        var sentenceQuery = _context.Sentences
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            sentenceQuery = sentenceQuery.Where(s =>
                EF.Functions.ILike(s.Text, pattern)
                || EF.Functions.ILike(s.Meaning, pattern));
        }

        if (level.HasValue)
        {
            sentenceQuery = sentenceQuery.Where(s => s.Level == level.Value);
        }

        if (!string.IsNullOrWhiteSpace(createdBy))
        {
            sentenceQuery = sentenceQuery.Where(s => s.CreatedBy == createdBy);
        }

        if (hasAudio.HasValue)
        {
            if (hasAudio.Value)
            {
                sentenceQuery = sentenceQuery.Where(s => !string.IsNullOrWhiteSpace(s.AudioUrl));
            }
            else
            {
                sentenceQuery = sentenceQuery.Where(s => string.IsNullOrWhiteSpace(s.AudioUrl));
            }
        }

        var total = await sentenceQuery.CountAsync();

        var items = await sentenceQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
