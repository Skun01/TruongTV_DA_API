using Application.DTOs.Learning;
using Domain.Entities;

namespace Application.Mappings;

public static class LearningMappings
{
    public static StudySessionResponse ToResponse(this StudySession session, Deck deck)
    {
        var completedCards = session.CompletedCardIds.Count;
        var totalCards = session.CardIds.Count;

        return new StudySessionResponse
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckTitle = deck.Title,
            Mode = session.Mode.ToString(),
            FolderIds = session.SelectedFolderIds.ToList(),
            TotalCards = totalCards,
            CompletedCards = completedCards,
            RemainingCards = Math.Max(totalCards - completedCards, 0),
            CorrectCount = session.CorrectCount,
            IncorrectCount = session.IncorrectCount,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            Settings = new StudySessionSettingsResponse
            {
                FlashcardFront = session.FlashcardFront.ToString(),
                FlashcardBack = session.FlashcardBack.ToString(),
                MultipleChoiceQuestion = session.MultipleChoiceQuestion.ToString(),
                ShuffleOptions = session.ShuffleOptions,
            },
        };
    }
}
