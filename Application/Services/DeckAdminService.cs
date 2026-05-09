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

public class DeckAdminService : IDeckAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public DeckAdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<AdminDeckListItemResponse> Items, MetaData Meta)> SearchAsync(AdminDeckListQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
        var (items, total) = await _unitOfWork.Decks.SearchAdminAsync(
            query.Q,
            query.TypeId,
            query.CreatedBy,
            query.Status,
            query.Visibility,
            query.IsOfficial,
            page,
            pageSize);

        return (
            items.Select(d => d.ToAdminListItemResponse()).ToList(),
            new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
            });
    }

    public async Task<AdminDeckDetailResponse> GetDetailAsync(string deckId)
    {
        var deck = await _unitOfWork.Decks.GetAdminDetailByIdAsync(deckId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        return deck.ToAdminDetailResponse();
    }

    public async Task<AdminDeckDetailResponse> CreateAsync(CreateAdminDeckRequest request, string currentUserId)
    {
        await EnsureDeckTypeExistsIfNeeded(request.TypeId);

        var ownerId = StringHelper.NormalizeOptional(request.CreatedBy) ?? currentUserId;
        await EnsureUserExists(ownerId);

        var deck = new Deck
        {
            Id = Guid.NewGuid().ToString(),
            CreatedBy = ownerId,
            TypeId = StringHelper.NormalizeOptional(request.TypeId),
            Title = request.Title.Trim(),
            Description = StringHelper.NormalizeOptional(request.Description) ?? string.Empty,
            CoverImageUrl = StringHelper.NormalizeOptional(request.CoverImageUrl),
            Visibility = DeckHelper.ParseVisibilityOrDefault(request.Visibility, DeckVisibility.Public),
            Status = DeckHelper.ParseStatusOrDefault(request.Status, PublishStatus.Draft),
            IsOfficial = request.IsOfficial,
            CardsCount = 0,
            FoldersCount = 0,
        };

        await _unitOfWork.Decks.AddAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        var createdDeck = await _unitOfWork.Decks.GetManagedDetailByIdAsync(deck.Id);
        if (createdDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return createdDeck.ToAdminDetailResponse();
    }

    public async Task<AdminDeckDetailResponse> UpdateAsync(string deckId, UpdateAdminDeckRequest request)
    {
        var deck = await _unitOfWork.Decks.GetManagedByIdAsync(deckId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        if (request.TypeId != null)
            await EnsureDeckTypeExistsIfNeeded(request.TypeId);

        if (request.Title != null)
            deck.Title = request.Title.Trim();

        if (request.Description != null)
            deck.Description = request.Description.Trim();

        if (request.CoverImageUrl != null)
            deck.CoverImageUrl = StringHelper.NormalizeOptional(request.CoverImageUrl);

        if (request.Visibility != null)
            deck.Visibility = DeckHelper.ParseVisibilityOrDefault(request.Visibility, deck.Visibility);

        if (request.Status != null)
            deck.Status = DeckHelper.ParseStatusOrDefault(request.Status, deck.Status);

        if (request.IsOfficial.HasValue)
            deck.IsOfficial = request.IsOfficial.Value;

        if (request.TypeId != null)
            deck.TypeId = StringHelper.NormalizeOptional(request.TypeId);

        deck.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Decks.UpdateAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        var updatedDeck = await _unitOfWork.Decks.GetManagedDetailByIdAsync(deckId);
        if (updatedDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedDeck.ToAdminDetailResponse();
    }

    public async Task<bool> DeleteAsync(string deckId)
    {
        var deck = await _unitOfWork.Decks.GetManagedByIdAsync(deckId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        _unitOfWork.Decks.DeleteAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task<AdminDeckDetailResponse> PublishAsync(string deckId)
    {
        return UpdateStatusAsync(deckId, PublishStatus.Published);
    }

    public Task<AdminDeckDetailResponse> ArchiveAsync(string deckId)
    {
        return UpdateStatusAsync(deckId, PublishStatus.Archived);
    }

    public Task<AdminDeckDetailResponse> UnpublishAsync(string deckId)
    {
        return UpdateStatusAsync(deckId, PublishStatus.Draft);
    }

    public async Task<DeckFolderResponse> CreateFolderAsync(string deckId, CreateDeckFolderRequest request)
    {
        var deck = await _unitOfWork.Decks.GetManagedByIdAsync(deckId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        var folder = new DeckFolder
        {
            Id = Guid.NewGuid().ToString(),
            DeckId = deckId,
            Title = request.Title.Trim(),
            Description = StringHelper.NormalizeOptional(request.Description) ?? string.Empty,
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

    public async Task<DeckFolderResponse> UpdateFolderAsync(string folderId, UpdateDeckFolderRequest request)
    {
        var folder = await _unitOfWork.Decks.GetFolderWithCardsByIdAsync(folderId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        folder.Title = request.Title.Trim();
        folder.Description = StringHelper.NormalizeOptional(request.Description) ?? string.Empty;
        folder.UpdatedAt = DateTime.UtcNow;
        folder.Deck.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.DeckFolders.UpdateAsync(folder);
        _unitOfWork.Decks.UpdateAsync(folder.Deck);
        await _unitOfWork.SaveChangesAsync();

        return folder.ToResponse();
    }

    public async Task<bool> DeleteFolderAsync(string folderId)
    {
        var folder = await _unitOfWork.Decks.GetFolderWithCardsByIdAsync(folderId);
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

    public async Task<DeckFolderResponse> AddCardToFolderAsync(string folderId, AddCardToFolderRequest request)
    {
        var folder = await _unitOfWork.Decks.GetFolderWithCardsByIdAsync(folderId);
        if (folder == null)
            throw new ApplicationException(MessageConstants.DeckMessage.FOLDER_NOT_FOUND);

        var card = await _unitOfWork.Cards.GetByIdAsync(request.CardId);
        if (card == null)
            throw new ApplicationException(MessageConstants.DeckMessage.CARD_NOT_FOUND);

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

        var updatedFolder = await _unitOfWork.Decks.GetFolderWithCardsByIdAsync(folderId);
        if (updatedFolder == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedFolder.ToResponse();
    }

    public async Task<bool> RemoveCardFromFolderAsync(string folderId, string cardId)
    {
        var folder = await _unitOfWork.Decks.GetFolderWithCardsByIdAsync(folderId);
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

    public async Task<List<DeckFolderResponse>> ReorderDeckFoldersAsync(string deckId, ReorderDeckFoldersRequest request)
    {
        var deck = await _unitOfWork.Decks.GetManagedByIdAsync(deckId);
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

        var updatedDeck = await _unitOfWork.Decks.GetManagedDetailByIdAsync(deckId);
        if (updatedDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedDeck.Folders
            .OrderBy(f => f.Position)
            .Select(f => f.ToResponse())
            .ToList();
    }

    public async Task<List<DeckFolderCardItemResponse>> ReorderFolderCardsAsync(string folderId, ReorderFolderCardsRequest request)
    {
        var folder = await _unitOfWork.Decks.GetFolderWithCardsByIdAsync(folderId);
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

    private async Task<AdminDeckDetailResponse> UpdateStatusAsync(string deckId, PublishStatus status)
    {
        var deck = await _unitOfWork.Decks.GetManagedByIdAsync(deckId);
        if (deck == null)
            throw new ApplicationException(MessageConstants.DeckMessage.NOT_FOUND);

        deck.Status = status;
        deck.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Decks.UpdateAsync(deck);
        await _unitOfWork.SaveChangesAsync();

        var updatedDeck = await _unitOfWork.Decks.GetManagedDetailByIdAsync(deckId);
        if (updatedDeck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return updatedDeck.ToAdminDetailResponse();
    }

    private async Task EnsureDeckTypeExistsIfNeeded(string? typeId)
    {
        var normalizedTypeId = StringHelper.NormalizeOptional(typeId);
        if (normalizedTypeId == null)
            return;

        var deckType = await _unitOfWork.DeckTypes.GetByIdAsync(normalizedTypeId);
        if (deckType == null)
            throw new ApplicationException(MessageConstants.DeckTypeMessage.NOT_FOUND);
    }

    private async Task EnsureUserExists(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);
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
