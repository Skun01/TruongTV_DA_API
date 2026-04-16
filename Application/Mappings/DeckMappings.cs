using Application.DTOs.Decks;
using Domain.Entities;

namespace Application.Mappings;

public static class DeckMappings
{
    public static DeckTypeResponse ToResponse(this DeckType deckType)
    {
        return new DeckTypeResponse
        {
            Id = deckType.Id,
            Name = deckType.Name,
        };
    }

    public static DeckListItemResponse ToListItemResponse(this Deck deck, string? currentUserId)
    {
        return new DeckListItemResponse
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            CoverImageUrl = deck.CoverImageUrl,
            Visibility = deck.Visibility.ToString(),
            Status = deck.Status.ToString(),
            IsOfficial = deck.IsOfficial,
            CardsCount = deck.CardsCount,
            FoldersCount = deck.FoldersCount,
            Type = deck.Type.ToSummaryResponse(),
            CreatedBy = deck.Creator.ToResponse(),
            ForkedFromId = deck.ForkedFromId,
            IsBookmarked = deck.Bookmarks.Any(b => string.IsNullOrWhiteSpace(currentUserId) ? false : b.UserId == currentUserId),
            IsOwner = !string.IsNullOrWhiteSpace(currentUserId) && deck.CreatedBy == currentUserId,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt,
        };
    }

    public static DeckDetailResponse ToDetailResponse(this Deck deck, string? currentUserId)
    {
        return new DeckDetailResponse
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            CoverImageUrl = deck.CoverImageUrl,
            Visibility = deck.Visibility.ToString(),
            Status = deck.Status.ToString(),
            IsOfficial = deck.IsOfficial,
            CardsCount = deck.CardsCount,
            FoldersCount = deck.FoldersCount,
            Type = deck.Type.ToSummaryResponse(),
            CreatedBy = deck.Creator.ToResponse(),
            ForkedFromId = deck.ForkedFromId,
            IsBookmarked = deck.Bookmarks.Any(b => string.IsNullOrWhiteSpace(currentUserId) ? false : b.UserId == currentUserId),
            IsOwner = !string.IsNullOrWhiteSpace(currentUserId) && deck.CreatedBy == currentUserId,
            Folders = deck.Folders
                .OrderBy(f => f.Position)
                .Select(f => f.ToResponse())
                .ToList(),
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt,
        };
    }

    public static DeckFolderResponse ToResponse(this DeckFolder folder)
    {
        return new DeckFolderResponse
        {
            Id = folder.Id,
            Title = folder.Title,
            Description = folder.Description,
            Position = folder.Position,
            CardsCount = folder.CardsCount,
            Cards = folder.FolderCards
                .OrderBy(fc => fc.Position)
                .Select(fc => fc.ToResponse())
                .ToList(),
        };
    }

    public static DeckFolderCardItemResponse ToResponse(this FolderCard folderCard)
    {
        return new DeckFolderCardItemResponse
        {
            CardId = folderCard.CardId,
            Position = folderCard.Position,
            AddedAt = folderCard.AddedAt,
            Card = folderCard.Card.ToSummaryResponse(),
        };
    }

    public static DeckBookmarkResponse ToResponse(this DeckBookmark? bookmark, string deckId, bool bookmarked)
    {
        return new DeckBookmarkResponse
        {
            DeckId = deckId,
            Bookmarked = bookmarked,
            SavedAt = bookmark?.SavedAt,
        };
    }

    public static DeckCardSummaryResponse ToSummaryResponse(this Card card)
    {
        return new DeckCardSummaryResponse
        {
            Id = card.Id,
            Title = card.Title,
            Summary = card.Summary,
            CardType = card.CardType.ToString(),
            Level = card.Level?.ToString(),
        };
    }

    public static DeckCreatorResponse ToResponse(this User user)
    {
        return new DeckCreatorResponse
        {
            Id = user.Id,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
        };
    }

    public static DeckTypeSummaryResponse ToSummaryResponse(this DeckType? deckType)
    {
        return new DeckTypeSummaryResponse
        {
            Id = deckType?.Id,
            Name = deckType?.Name,
        };
    }
}
