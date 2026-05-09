using Application.DTOs.Learning;
using Application.Helper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings;

public static class LearningMappings
{
    public static StudySessionResponse ToResponse(this StudySession session, Deck? deck)
    {
        var completedCards = session.CompletedCardIds.Count;
        var totalCards = session.CardIds.Count;

        return new StudySessionResponse
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckTitle = deck?.Title,
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

    public static CardProgressResponse ToCardProgressResponse(this Card card, UserCardProgress progress)
    {
        return new CardProgressResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            SrsLevel = progress.SrsLevel.ToString(),
            NextReviewAt = progress.NextReviewAt,
            LastReviewedAt = progress.LastReviewedAt,
            ConsecutiveCorrect = progress.ConsecutiveCorrect,
            IsMastered = progress.IsMastered,
            LastSentenceId = progress.LastSentenceId,
        };
    }

    public static CardProgressResponse ToDefaultCardProgressResponse(this Card card, DateTime nextReviewAt)
    {
        return new CardProgressResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            SrsLevel = SrsLevel.level_1.ToString(),
            NextReviewAt = nextReviewAt,
            ConsecutiveCorrect = 0,
            IsMastered = false,
        };
    }

    public static StudySessionResultResponse ToResultResponse(this StudySession session, Deck? deck)
    {
        var attempts = session.CorrectCount + session.IncorrectCount;

        return new StudySessionResultResponse
        {
            SessionId = session.Id,
            DeckId = session.DeckId,
            DeckTitle = deck?.Title,
            Mode = session.Mode.ToString(),
            TotalCards = session.CardIds.Count,
            CompletedCards = session.CompletedCardIds.Count,
            CorrectCount = session.CorrectCount,
            IncorrectCount = session.IncorrectCount,
            Accuracy = LearningHelper.CalculateAccuracy(session.CorrectCount, attempts),
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
