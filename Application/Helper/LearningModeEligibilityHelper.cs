using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class LearningModeEligibilityHelper
{
    public static bool IsCardEligible(Card card, StudyMode mode)
    {
        return mode switch
        {
            StudyMode.FillInBlank => IsFillInBlankReady(card),
            StudyMode.MultipleChoice => IsMultipleChoiceReady(card),
            StudyMode.Flashcard => IsFlashcardReady(card),
            _ => false,
        };
    }

    public static bool IsFillInBlankReady(Card card)
    {
        if (card.CardType == CardType.Kanji)
            return LearningHelper.BuildFallbackAnswers(card).Count > 0;

        var cardSentences = GetAttachedSentences(card);
        return HasValidSentencePositions(cardSentences)
            && cardSentences.Count > 0
            && cardSentences.All(IsFillInBlankSentenceReady);
    }

    public static bool IsMultipleChoiceReady(Card card)
    {
        return !string.IsNullOrWhiteSpace(card.Title)
            && !string.IsNullOrWhiteSpace(card.Summary);
    }

    public static bool IsFlashcardReady(Card card)
    {
        return !string.IsNullOrWhiteSpace(card.Title)
            && !string.IsNullOrWhiteSpace(card.Summary);
    }

    public static CardSentence? GetFirstFillInBlankSentence(Card card)
    {
        return card.CardSentences
            .Where(IsFillInBlankSentenceReady)
            .OrderBy(cs => cs.Position)
            .FirstOrDefault();
    }

    public static bool IsFillInBlankSentenceReady(CardSentence cardSentence)
    {
        if (cardSentence.Position <= 0 || cardSentence.Sentence == null)
            return false;

        var acceptedAnswers = StringHelper.NormalizeAnswerList(cardSentence.AnswerList, cardSentence.BlankWord);
        if (acceptedAnswers.Count == 0)
            return false;

        return string.IsNullOrWhiteSpace(cardSentence.BlankWord)
            || cardSentence.Sentence.Text.Contains(cardSentence.BlankWord, StringComparison.OrdinalIgnoreCase);
    }

    private static List<CardSentence> GetAttachedSentences(Card card)
    {
        return card.CardSentences
            .Where(cs => cs.Sentence != null)
            .ToList();
    }

    private static bool HasValidSentencePositions(List<CardSentence> cardSentences)
    {
        return cardSentences.Count > 0
            && cardSentences.All(cs => cs.Position > 0)
            && cardSentences.Select(cs => cs.Position).Distinct().Count() == cardSentences.Count;
    }
}
