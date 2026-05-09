using Application.Common;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class LearningSessionHelper
{
    public static StudySession CreateSession(
        string userId,
        string? deckId,
        StudyMode mode,
        LearningSessionSettings settings,
        List<string> selectedFolderIds,
        List<string> cardIds,
        List<string>? skippedCardIds = null)
    {
        return new StudySession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            DeckId = deckId,
            Mode = mode,
            FlashcardFront = settings.FlashcardFront,
            FlashcardBack = settings.FlashcardBack,
            MultipleChoiceQuestion = settings.MultipleChoiceQuestion,
            ShuffleOptions = settings.ShuffleOptions,
            SelectedFolderIds = selectedFolderIds,
            CardIds = cardIds,
            SkippedCardIds = skippedCardIds ?? new List<string>(),
        };
    }

    public static List<string> ResolveFolderScope(Deck deck, List<string>? requestedFolderIds)
    {
        var allFolderIds = deck.Folders.Select(folder => folder.Id).ToList();
        if (requestedFolderIds == null || requestedFolderIds.Count == 0)
            return allFolderIds;

        var normalizedFolderIds = LearningHelper.NormalizeRequestedIds(requestedFolderIds);
        if (normalizedFolderIds.Count == 0)
            return allFolderIds;

        if (normalizedFolderIds.Any(folderId => !allFolderIds.Contains(folderId, StringComparer.Ordinal)))
            throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

        return normalizedFolderIds;
    }

    public static List<string> ResolveCardScope(Deck deck, List<string>? requestedCardIds)
    {
        var allCardIds = deck.Folders
            .SelectMany(folder => folder.FolderCards)
            .OrderBy(folderCard => folderCard.Position)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (requestedCardIds == null || requestedCardIds.Count == 0)
            return allCardIds;

        var normalizedCardIds = LearningHelper.NormalizeRequestedIds(requestedCardIds);
        if (normalizedCardIds.Count == 0)
            return allCardIds;

        if (normalizedCardIds.Any(cardId => !allCardIds.Contains(cardId, StringComparer.Ordinal)))
            throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

        return normalizedCardIds;
    }

    public static List<string> ResolveSelectedFolderIds(Deck deck, List<string> cardIds)
    {
        return deck.Folders
            .Where(folder => folder.FolderCards.Any(folderCard => cardIds.Contains(folderCard.CardId, StringComparer.Ordinal)))
            .Select(folder => folder.Id)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
