using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Card?> GetVocabularyDetailByIdAsync(string cardId)
    {
        return await _context.Cards
            .AsNoTracking()
            .Include(c => c.VocabularyDetail)
            .Include(c => c.CardSentences)
                .ThenInclude(cs => cs.Sentence)
            .FirstOrDefaultAsync(c => c.Id == cardId);
    }

    public async Task<(List<Card> Items, int Total)> SearchVocabularyAsync(
        string? query,
        JlptLevel? level,
        PublishStatus? status,
        WordType? wordType,
        bool? hasAudio,
        string? createdBy,
        int page,
        int pageSize)
    {
        var cardsQuery = _context.Cards
            .AsNoTracking()
            .Include(c => c.VocabularyDetail)
            .Where(c => c.CardType == CardType.Vocab);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            cardsQuery = cardsQuery.Where(c =>
                EF.Functions.ILike(c.Title, pattern)
                || EF.Functions.ILike(c.Summary, pattern)
                || (c.VocabularyDetail != null
                    && (EF.Functions.ILike(c.VocabularyDetail.Writing, pattern)
                        || (c.VocabularyDetail.Reading != null && EF.Functions.ILike(c.VocabularyDetail.Reading, pattern)))));
        }

        if (level.HasValue)
            cardsQuery = cardsQuery.Where(c => c.Level == level.Value);

        if (status.HasValue)
            cardsQuery = cardsQuery.Where(c => c.Status == status.Value);

        if (wordType.HasValue)
            cardsQuery = cardsQuery.Where(c => c.VocabularyDetail != null && c.VocabularyDetail.WordType == wordType.Value);

        if (hasAudio.HasValue)
        {
            if (hasAudio.Value)
                cardsQuery = cardsQuery.Where(c => c.VocabularyDetail != null && !string.IsNullOrWhiteSpace(c.VocabularyDetail.AudioUrl));
            else
                cardsQuery = cardsQuery.Where(c => c.VocabularyDetail == null || string.IsNullOrWhiteSpace(c.VocabularyDetail.AudioUrl));
        }

        if (!string.IsNullOrWhiteSpace(createdBy))
            cardsQuery = cardsQuery.Where(c => c.CreatedBy == createdBy);

        var total = await cardsQuery.CountAsync();

        var items = await cardsQuery
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Card>> GetVocabularyExportAsync(
        string? query,
        JlptLevel? level,
        PublishStatus? status,
        WordType? wordType,
        bool? hasAudio,
        string? createdBy)
    {
        var cardsQuery = _context.Cards
            .AsNoTracking()
            .Include(c => c.VocabularyDetail)
            .Include(c => c.CardSentences)
                .ThenInclude(cs => cs.Sentence)
            .Where(c => c.CardType == CardType.Vocab);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            cardsQuery = cardsQuery.Where(c =>
                EF.Functions.ILike(c.Title, pattern)
                || EF.Functions.ILike(c.Summary, pattern)
                || (c.VocabularyDetail != null
                    && (EF.Functions.ILike(c.VocabularyDetail.Writing, pattern)
                        || (c.VocabularyDetail.Reading != null && EF.Functions.ILike(c.VocabularyDetail.Reading, pattern)))));
        }

        if (level.HasValue)
            cardsQuery = cardsQuery.Where(c => c.Level == level.Value);

        if (status.HasValue)
            cardsQuery = cardsQuery.Where(c => c.Status == status.Value);

        if (wordType.HasValue)
            cardsQuery = cardsQuery.Where(c => c.VocabularyDetail != null && c.VocabularyDetail.WordType == wordType.Value);

        if (hasAudio.HasValue)
        {
            if (hasAudio.Value)
                cardsQuery = cardsQuery.Where(c => c.VocabularyDetail != null && !string.IsNullOrWhiteSpace(c.VocabularyDetail.AudioUrl));
            else
                cardsQuery = cardsQuery.Where(c => c.VocabularyDetail == null || string.IsNullOrWhiteSpace(c.VocabularyDetail.AudioUrl));
        }

        if (!string.IsNullOrWhiteSpace(createdBy))
            cardsQuery = cardsQuery.Where(c => c.CreatedBy == createdBy);

        return await cardsQuery
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<Card> Items, int Total)> SearchCardsAsync(
        CardType? cardType,
        string? query,
        JlptLevel? level,
        int page,
        int pageSize)
    {
        var cardsQuery = _context.Cards
            .AsNoTracking()
            .Where(c => c.Status == PublishStatus.Published)
            .AsQueryable();

        if (cardType.HasValue)
            cardsQuery = cardsQuery.Where(c => c.CardType == cardType.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            cardsQuery = cardsQuery.Where(c =>
                EF.Functions.ILike(c.Title, pattern)
                || EF.Functions.ILike(c.Summary, pattern));
        }

        if (level.HasValue)
            cardsQuery = cardsQuery.Where(c => c.Level == level.Value);

        var total = await cardsQuery.CountAsync();

        var items = await cardsQuery
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
