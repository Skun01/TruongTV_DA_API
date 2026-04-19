using Application.IRepositories;
using Application.Helper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DeckRepository : Repository<Deck>, IDeckRepository
{
    public DeckRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Deck> Items, int Total)> SearchPublicAsync(string? query, string? typeId, bool? officialOnly, int page, int pageSize, string? currentUserId)
    {
        var deckQuery = BuildDeckSummaryQuery(currentUserId)
            .Where(d => d.Status == PublishStatus.Published && d.Visibility == DeckVisibility.Public);

        deckQuery = ApplyCommonFilters(deckQuery, query, typeId, officialOnly);

        var total = await deckQuery.CountAsync();
        var items = await deckQuery
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Deck> Items, int Total)> SearchOwnedByUserAsync(string userId, string? query, string? typeId, int page, int pageSize)
    {
        var deckQuery = BuildDeckSummaryQuery(userId)
            .Where(d => d.CreatedBy == userId);

        deckQuery = ApplyCommonFilters(deckQuery, query, typeId, null);

        var total = await deckQuery.CountAsync();
        var items = await deckQuery
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Deck> Items, int Total)> SearchBookmarkedByUserAsync(string userId, string? query, string? typeId, int page, int pageSize)
    {
        var deckQuery = BuildDeckSummaryQuery(userId)
            .Where(d => d.Bookmarks.Any(b => b.UserId == userId))
            .Where(d => (d.Status == PublishStatus.Published && d.Visibility == DeckVisibility.Public) || d.CreatedBy == userId);

        deckQuery = ApplyCommonFilters(deckQuery, query, typeId, null);

        var total = await deckQuery.CountAsync();
        var items = await deckQuery
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Deck> Items, int Total)> SearchAdminAsync(string? query, string? typeId, string? createdBy, string? status, string? visibility, bool? isOfficial, int page, int pageSize)
    {
        var deckQuery = BuildAdminDeckSummaryQuery();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            deckQuery = deckQuery.Where(d =>
                EF.Functions.ILike(d.Title, pattern)
                || EF.Functions.ILike(d.Description, pattern));
        }

        if (!string.IsNullOrWhiteSpace(typeId))
            deckQuery = deckQuery.Where(d => d.TypeId == typeId);

        if (!string.IsNullOrWhiteSpace(createdBy))
            deckQuery = deckQuery.Where(d => d.CreatedBy == createdBy);

        var parsedStatus = EnumParsingHelper.ParseNullable<PublishStatus>(status);
        if (parsedStatus.HasValue)
            deckQuery = deckQuery.Where(d => d.Status == parsedStatus.Value);

        var parsedVisibility = EnumParsingHelper.ParseNullable<DeckVisibility>(visibility);
        if (parsedVisibility.HasValue)
            deckQuery = deckQuery.Where(d => d.Visibility == parsedVisibility.Value);

        if (isOfficial.HasValue)
            deckQuery = deckQuery.Where(d => d.IsOfficial == isOfficial.Value);

        var total = await deckQuery.CountAsync();
        var items = await deckQuery
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Deck?> GetDetailByIdAsync(string deckId, string? currentUserId)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Bookmarks.Where(b => currentUserId != null && b.UserId == currentUserId))
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
                    .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<Deck?> GetAdminDetailByIdAsync(string deckId)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Bookmarks)
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
                    .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<Deck?> GetForkSourceByIdAsync(string deckId)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<Deck?> GetManagedByIdAsync(string deckId)
    {
        return await _context.Decks
            .Include(d => d.Folders)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<Deck?> GetManagedDetailByIdAsync(string deckId)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Bookmarks)
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
                    .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<Deck?> GetOwnedByIdAsync(string deckId, string userId)
    {
        return await _context.Decks
            .Include(d => d.Folders)
            .FirstOrDefaultAsync(d => d.Id == deckId && d.CreatedBy == userId);
    }

    public async Task<DeckFolder?> GetFolderByIdAsync(string folderId)
    {
        return await _context.DeckFolders
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == folderId);
    }

    public async Task<DeckFolder?> GetFolderWithCardsByIdAsync(string folderId)
    {
        return await _context.DeckFolders
            .Include(f => f.Deck)
            .Include(f => f.FolderCards)
                .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(f => f.Id == folderId);
    }

    public async Task<Deck?> GetOwnedDetailByIdAsync(string deckId, string userId)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Bookmarks.Where(b => b.UserId == userId))
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
                    .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId && d.CreatedBy == userId);
    }

    public async Task<DeckFolder?> GetOwnedFolderByIdAsync(string folderId, string userId)
    {
        return await _context.DeckFolders
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == folderId && f.Deck.CreatedBy == userId);
    }

    public async Task<DeckFolder?> GetOwnedFolderWithCardsByIdAsync(string folderId, string userId)
    {
        return await _context.DeckFolders
            .Include(f => f.Deck)
            .Include(f => f.FolderCards)
                .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(f => f.Id == folderId && f.Deck.CreatedBy == userId);
    }

    public async Task<FolderCard?> GetFolderCardAsync(string folderId, string cardId)
    {
        return await _context.FolderCards
            .FirstOrDefaultAsync(fc => fc.FolderId == folderId && fc.CardId == cardId);
    }

    public async Task<bool> ExistsCardInDeckAsync(string deckId, string cardId)
    {
        return await _context.FolderCards
            .AnyAsync(fc => fc.DeckId == deckId && fc.CardId == cardId);
    }

    public async Task<bool> ExistsByTypeIdAsync(string typeId)
    {
        return await _context.Decks.AnyAsync(d => d.TypeId == typeId);
    }

    public async Task<List<Deck>> GetReadableDecksContainingCardIdsAsync(string userId, List<string> cardIds)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
            .Where(d => ((d.Status == PublishStatus.Published && d.Visibility == DeckVisibility.Public) || d.CreatedBy == userId)
                && d.Folders.Any(f => f.FolderCards.Any(fc => cardIds.Contains(fc.CardId))))
            .ToListAsync();
    }

    public async Task<List<Deck>> GetAdminDecksContainingCardIdsAsync(List<string> cardIds)
    {
        return await _context.Decks
            .AsNoTracking()
            .Include(d => d.Folders)
                .ThenInclude(f => f.FolderCards)
            .Where(d => d.Folders.Any(f => f.FolderCards.Any(fc => cardIds.Contains(fc.CardId))))
            .ToListAsync();
    }

    private IQueryable<Deck> BuildDeckSummaryQuery(string? currentUserId)
    {
        return _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Bookmarks.Where(b => currentUserId != null && b.UserId == currentUserId));
    }

    private IQueryable<Deck> BuildAdminDeckSummaryQuery()
    {
        return _context.Decks
            .AsNoTracking()
            .Include(d => d.Type)
            .Include(d => d.Creator)
            .Include(d => d.Bookmarks);
    }

    private static IQueryable<Deck> ApplyCommonFilters(IQueryable<Deck> deckQuery, string? query, string? typeId, bool? officialOnly)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            deckQuery = deckQuery.Where(d =>
                EF.Functions.ILike(d.Title, pattern)
                || EF.Functions.ILike(d.Description, pattern));
        }

        if (!string.IsNullOrWhiteSpace(typeId))
            deckQuery = deckQuery.Where(d => d.TypeId == typeId);

        if (officialOnly == true)
            deckQuery = deckQuery.Where(d => d.IsOfficial);

        return deckQuery;
    }
}
