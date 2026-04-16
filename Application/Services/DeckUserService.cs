using Application.Common;
using Application.DTOs.Decks;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DeckUserService : IDeckUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public DeckUserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<DeckListItemResponse> Items, MetaData Meta)> SearchPublicAsync(DeckListQuery query, string? currentUserId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var (items, total) = await _unitOfWork.Decks.SearchPublicAsync(
            query.Q,
            query.TypeId,
            query.OfficialOnly,
            page,
            pageSize,
            currentUserId);

        return (
            items.Select(d => d.ToListItemResponse(currentUserId)).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<DeckDetailResponse> GetDetailAsync(string deckId, string? currentUserId)
    {
        var deck = await _unitOfWork.Decks.GetDetailByIdAsync(deckId, currentUserId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        EnsureDeckReadable(deck, currentUserId);

        return deck.ToDetailResponse(currentUserId);
    }

    public async Task<DeckBookmarkResponse> ToggleBookmarkAsync(string deckId, string userId, ToggleDeckBookmarkRequest request)
    {
        var deck = await _unitOfWork.Decks.GetByIdAsync(deckId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        EnsureDeckReadable(deck, userId);

        var existingBookmark = await _unitOfWork.DeckBookmarks.GetByUserAndDeckIdAsync(userId, deckId);
        if (request.Bookmarked)
        {
            if (existingBookmark == null)
            {
                existingBookmark = new DeckBookmark
                {
                    UserId = userId,
                    DeckId = deckId,
                };

                await _unitOfWork.DeckBookmarks.AddAsync(existingBookmark);
                await _unitOfWork.SaveChangesAsync();
            }

            return existingBookmark.ToResponse(deckId, true);
        }

        if (existingBookmark != null)
        {
            _unitOfWork.DeckBookmarks.DeleteAsync(existingBookmark);
            await _unitOfWork.SaveChangesAsync();
        }

        return ((DeckBookmark?)null).ToResponse(deckId, false);
    }

    public async Task<(List<DeckListItemResponse> Items, MetaData Meta)> GetBookmarkedAsync(BookmarkedDeckListQuery query, string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var (items, total) = await _unitOfWork.Decks.SearchBookmarkedByUserAsync(
            userId,
            query.Q,
            query.TypeId,
            page,
            pageSize);

        return (
            items.Select(d => d.ToListItemResponse(userId)).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<DeckDetailResponse> ForkAsync(string deckId, string userId)
    {
        var sourceDeck = await _unitOfWork.Decks.GetForkSourceByIdAsync(deckId);
        if (sourceDeck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        if (sourceDeck.Status != PublishStatus.Published || sourceDeck.Visibility != DeckVisibility.Public)
            throw new ApplicationException(MessageConstants.DeckMessage.FORK_SOURCE_INVALID);

        var newDeckId = Guid.NewGuid().ToString();
        var orderedFolders = sourceDeck.Folders.OrderBy(f => f.Position).ToList();

        var forkedDeck = new Deck
        {
            Id = newDeckId,
            CreatedBy = userId,
            ForkedFromId = sourceDeck.Id,
            TypeId = sourceDeck.TypeId,
            Title = sourceDeck.Title,
            Description = sourceDeck.Description,
            CoverImageUrl = sourceDeck.CoverImageUrl,
            Visibility = DeckVisibility.Private,
            Status = PublishStatus.Published,
            IsOfficial = false,
            CardsCount = sourceDeck.CardsCount,
            FoldersCount = sourceDeck.FoldersCount,
        };

        await _unitOfWork.Decks.AddAsync(forkedDeck);

        foreach (var sourceFolder in orderedFolders)
        {
            var newFolderId = Guid.NewGuid().ToString();
            var forkedFolder = new DeckFolder
            {
                Id = newFolderId,
                DeckId = newDeckId,
                Title = sourceFolder.Title,
                Description = sourceFolder.Description,
                Position = sourceFolder.Position,
                CardsCount = sourceFolder.CardsCount,
            };

            await _unitOfWork.DeckFolders.AddAsync(forkedFolder);

            foreach (var sourceFolderCard in sourceFolder.FolderCards.OrderBy(fc => fc.Position))
            {
                await _unitOfWork.FolderCards.AddAsync(new FolderCard
                {
                    DeckId = newDeckId,
                    FolderId = newFolderId,
                    CardId = sourceFolderCard.CardId,
                    Position = sourceFolderCard.Position,
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var createdDeck = await _unitOfWork.Decks.GetDetailByIdAsync(newDeckId, userId);
        if (createdDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return createdDeck.ToDetailResponse(userId);
    }

    public async Task<(List<DeckListItemResponse> Items, MetaData Meta)> GetMyDecksAsync(MyDeckListQuery query, string userId)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var (items, total) = await _unitOfWork.Decks.SearchOwnedByUserAsync(
            userId,
            query.Q,
            query.TypeId,
            page,
            pageSize);

        return (
            items.Select(d => d.ToListItemResponse(userId)).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<DeckDetailResponse> CreateMyDeckAsync(CreateMyDeckRequest request, string userId)
    {
        if (!string.IsNullOrWhiteSpace(request.TypeId))
        {
            var deckType = await _unitOfWork.DeckTypes.GetByIdAsync(request.TypeId);
            if (deckType == null)
                throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);
        }

        var deck = new Deck
        {
            Id = Guid.NewGuid().ToString(),
            CreatedBy = userId,
            TypeId = NormalizeOptionalText(request.TypeId),
            Title = request.Title.Trim(),
            Description = NormalizeOptionalText(request.Description) ?? string.Empty,
            CoverImageUrl = NormalizeOptionalText(request.CoverImageUrl),
            Visibility = ParseVisibilityOrDefault(request.Visibility),
            Status = PublishStatus.Published,
            IsOfficial = false,
            CardsCount = 0,
            FoldersCount = 0,
        };

        await _unitOfWork.Decks.AddAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        var createdDeck = await _unitOfWork.Decks.GetOwnedDetailByIdAsync(deck.Id, userId);
        if (createdDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return createdDeck.ToDetailResponse(userId);
    }

    public async Task<DeckDetailResponse> UpdateMyDeckAsync(string deckId, UpdateMyDeckRequest request, string userId)
    {
        var deck = await _unitOfWork.Decks.GetOwnedByIdAsync(deckId, userId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        if (!string.IsNullOrWhiteSpace(request.TypeId))
        {
            var deckType = await _unitOfWork.DeckTypes.GetByIdAsync(request.TypeId);
            if (deckType == null)
                throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);
        }

        if (request.Title != null)
            deck.Title = request.Title.Trim();

        if (request.Description != null)
            deck.Description = request.Description.Trim();

        if (request.CoverImageUrl != null)
            deck.CoverImageUrl = NormalizeOptionalText(request.CoverImageUrl);

        if (request.Visibility != null)
            deck.Visibility = ParseVisibilityOrDefault(request.Visibility);

        if (request.TypeId != null)
            deck.TypeId = NormalizeOptionalText(request.TypeId);

        deck.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Decks.UpdateAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        var updatedDeck = await _unitOfWork.Decks.GetOwnedDetailByIdAsync(deckId, userId);
        if (updatedDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedDeck.ToDetailResponse(userId);
    }

    public async Task<bool> DeleteMyDeckAsync(string deckId, string userId)
    {
        var deck = await _unitOfWork.Decks.GetOwnedByIdAsync(deckId, userId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        _unitOfWork.Decks.DeleteAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DeckFolderResponse> CreateFolderAsync(string deckId, CreateDeckFolderRequest request, string userId)
    {
        var deck = await _unitOfWork.Decks.GetOwnedByIdAsync(deckId, userId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        var folder = new DeckFolder
        {
            Id = Guid.NewGuid().ToString(),
            DeckId = deckId,
            Title = request.Title.Trim(),
            Description = NormalizeOptionalText(request.Description) ?? string.Empty,
            Position = request.Position ?? GetNextFolderPosition(deck),
            CardsCount = 0,
        };

        await _unitOfWork.DeckFolders.AddAsync(folder);
        deck.FoldersCount += 1;
        deck.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Decks.UpdateAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        return folder.ToResponse();
    }

    public async Task<DeckFolderResponse> UpdateFolderAsync(string folderId, UpdateDeckFolderRequest request, string userId)
    {
        var folder = await _unitOfWork.Decks.GetOwnedFolderWithCardsByIdAsync(folderId, userId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        folder.Title = request.Title.Trim();
        folder.Description = NormalizeOptionalText(request.Description) ?? string.Empty;
        folder.UpdatedAt = DateTime.UtcNow;
        folder.Deck.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.DeckFolders.UpdateAsync(folder);
        _unitOfWork.Decks.UpdateAsync(folder.Deck);
        await _unitOfWork.SaveChangesAsync();

        return folder.ToResponse();
    }

    public async Task<bool> DeleteFolderAsync(string folderId, string userId)
    {
        var folder = await _unitOfWork.Decks.GetOwnedFolderWithCardsByIdAsync(folderId, userId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        var removedCards = folder.FolderCards.Count;
        folder.Deck.FoldersCount = Math.Max(0, folder.Deck.FoldersCount - 1);
        folder.Deck.CardsCount = Math.Max(0, folder.Deck.CardsCount - removedCards);
        folder.Deck.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.DeckFolders.DeleteAsync(folder);
        _unitOfWork.Decks.UpdateAsync(folder.Deck);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DeckFolderResponse> AddCardToFolderAsync(string folderId, AddCardToFolderRequest request, string userId)
    {
        var folder = await _unitOfWork.Decks.GetOwnedFolderWithCardsByIdAsync(folderId, userId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        var card = await _unitOfWork.Cards.GetByIdAsync(request.CardId);
        if (card == null)
            throw new ApplicationException(MessageConstants.DeckMessage.CARD_NOT_FOUND);

        if (card.Status != PublishStatus.Published && card.CreatedBy != userId)
            throw new ApplicationException(MessageConstants.DeckMessage.READ_FORBIDDEN);

        if (await _unitOfWork.Decks.ExistsCardInDeckAsync(folder.DeckId, request.CardId))
            throw new ApplicationException(MessageConstants.DeckMessage.CARD_DUPLICATED_IN_DECK);

        var folderCard = new FolderCard
        {
            DeckId = folder.DeckId,
            FolderId = folder.Id,
            CardId = request.CardId,
            Position = request.Position ?? GetNextCardPosition(folder),
        };

        await _unitOfWork.FolderCards.AddAsync(folderCard);
        folder.CardsCount += 1;
        folder.UpdatedAt = DateTime.UtcNow;
        folder.Deck.CardsCount += 1;
        folder.Deck.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.DeckFolders.UpdateAsync(folder);
        _unitOfWork.Decks.UpdateAsync(folder.Deck);
        await _unitOfWork.SaveChangesAsync();

        var updatedFolder = await _unitOfWork.Decks.GetOwnedFolderWithCardsByIdAsync(folderId, userId);
        if (updatedFolder == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedFolder.ToResponse();
    }

    public async Task<bool> RemoveCardFromFolderAsync(string folderId, string cardId, string userId)
    {
        var folder = await _unitOfWork.Decks.GetOwnedFolderWithCardsByIdAsync(folderId, userId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        var folderCard = folder.FolderCards.FirstOrDefault(fc => fc.CardId == cardId);
        if (folderCard == null)
            throw new ApplicationException(MessageConstants.DeckMessage.CARD_NOT_FOUND);

        _unitOfWork.FolderCards.DeleteAsync(folderCard);
        folder.CardsCount = Math.Max(0, folder.CardsCount - 1);
        folder.UpdatedAt = DateTime.UtcNow;
        folder.Deck.CardsCount = Math.Max(0, folder.Deck.CardsCount - 1);
        folder.Deck.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.DeckFolders.UpdateAsync(folder);
        _unitOfWork.Decks.UpdateAsync(folder.Deck);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<DeckFolderResponse>> ReorderDeckFoldersAsync(string deckId, ReorderDeckFoldersRequest request, string userId)
    {
        var deck = await _unitOfWork.Decks.GetOwnedByIdAsync(deckId, userId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        if (request.Items.Count != deck.Folders.Count
            || request.Items.Select(i => i.FolderId).Distinct().Count() != request.Items.Count
            || request.Items.Any(i => deck.Folders.All(f => f.Id != i.FolderId)))
            throw new ApplicationException(MessageConstants.DeckMessage.INVALID_REORDER_PAYLOAD);

        var positionMap = request.Items.ToDictionary(i => i.FolderId, i => i.Position);

        foreach (var folder in deck.Folders)
        {
            folder.Position = positionMap[folder.Id];
            folder.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.DeckFolders.UpdateAsync(folder);
        }

        deck.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Decks.UpdateAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        var updatedDeck = await _unitOfWork.Decks.GetOwnedDetailByIdAsync(deckId, userId);
        if (updatedDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedDeck.Folders
            .OrderBy(f => f.Position)
            .Select(f => f.ToResponse())
            .ToList();
    }

    public async Task<List<DeckFolderCardItemResponse>> ReorderFolderCardsAsync(string folderId, ReorderFolderCardsRequest request, string userId)
    {
        var folder = await _unitOfWork.Decks.GetOwnedFolderWithCardsByIdAsync(folderId, userId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        if (request.Items.Count != folder.FolderCards.Count
            || request.Items.Select(i => i.CardId).Distinct().Count() != request.Items.Count
            || request.Items.Any(i => folder.FolderCards.All(fc => fc.CardId != i.CardId)))
            throw new ApplicationException(MessageConstants.DeckMessage.INVALID_REORDER_PAYLOAD);

        var positionMap = request.Items.ToDictionary(i => i.CardId, i => i.Position);

        foreach (var folderCard in folder.FolderCards)
        {
            folderCard.Position = positionMap[folderCard.CardId];
            _unitOfWork.FolderCards.UpdateAsync(folderCard);
        }

        folder.UpdatedAt = DateTime.UtcNow;
        folder.Deck.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.DeckFolders.UpdateAsync(folder);
        _unitOfWork.Decks.UpdateAsync(folder.Deck);
        await _unitOfWork.SaveChangesAsync();

        return folder.FolderCards
            .OrderBy(fc => fc.Position)
            .Select(fc => fc.ToResponse())
            .ToList();
    }

    private static void EnsureDeckReadable(Deck deck, string? currentUserId)
    {
        var canRead = (deck.Status == PublishStatus.Published && deck.Visibility == DeckVisibility.Public)
            || (!string.IsNullOrWhiteSpace(currentUserId) && deck.CreatedBy == currentUserId);

        if (!canRead)
            throw new ApplicationException(MessageConstants.DeckMessage.READ_FORBIDDEN);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static DeckVisibility ParseVisibilityOrDefault(string? value)
    {
        return EnumParsingHelper.ParseNullable<DeckVisibility>(value) ?? DeckVisibility.Private;
    }

    private static int GetNextFolderPosition(Deck deck)
    {
        return deck.Folders.Count == 0 ? 1000 : deck.Folders.Max(f => f.Position) + 1000;
    }

    private static int GetNextCardPosition(DeckFolder folder)
    {
        return folder.FolderCards.Count == 0 ? 1000 : folder.FolderCards.Max(fc => fc.Position) + 1000;
    }
}
