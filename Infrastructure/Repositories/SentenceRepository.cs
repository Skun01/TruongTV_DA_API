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

    public async Task<(List<Sentence> Items, int Total)> SearchAsync(string? query, JlptLevel? level, int page, int pageSize)
    {
        var normalized = query?.Trim().ToLowerInvariant();

        var sentenceQuery = _context.Sentences
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            sentenceQuery = sentenceQuery.Where(s =>
                s.Text.ToLower().Contains(normalized) ||
                s.Meaning.ToLower().Contains(normalized));
        }

        if (level.HasValue)
        {
            sentenceQuery = sentenceQuery.Where(s => s.Level == level.Value);
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
