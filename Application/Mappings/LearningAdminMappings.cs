using Application.DTOs.LearningAdmin;
using Application.Helper;
using Domain.Entities;

namespace Application.Mappings;

public static class LearningAdminMappings
{
    public static LearningAdminCardSentenceConfigResponse ToAdminSentenceConfigResponse(this CardSentence cardSentence)
    {
        return new LearningAdminCardSentenceConfigResponse
        {
            SentenceId = cardSentence.SentenceId,
            Position = cardSentence.Position,
            Jp = cardSentence.Sentence?.Text ?? string.Empty,
            En = cardSentence.Sentence?.Meaning ?? string.Empty,
            AudioUrl = cardSentence.Sentence?.AudioUrl,
            Level = cardSentence.Sentence?.Level?.ToString(),
            BlankWord = cardSentence.BlankWord,
            Hint = cardSentence.Hint,
            AnswerList = StringHelper.NormalizeAnswerList(cardSentence.AnswerList, cardSentence.BlankWord),
        };
    }

    public static LearningAdminCardConfigResponse ToAdminCardConfigResponse(
        this Card card,
        List<LearningAdminCardIssueItemResponse> issues)
    {
        return new LearningAdminCardConfigResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            IsFillInBlankReady = LearningAdminHelper.IsFillInBlankReady(card, issues),
            IsMultipleChoiceReady = LearningAdminHelper.IsMultipleChoiceReady(issues),
            IsFlashcardReady = LearningAdminHelper.IsFlashcardReady(issues),
            AvailableModes = LearningAdminHelper.BuildAvailableModes(card, issues),
            Issues = issues,
            Sentences = card.CardSentences
                .Where(cs => cs.Sentence != null)
                .OrderBy(cs => cs.Position)
                .Select(cs => cs.ToAdminSentenceConfigResponse())
                .ToList(),
        };
    }

    public static LearningAdminCardIssueResponse ToAdminCardIssueResponse(
        this Card card,
        List<LearningAdminCardIssueItemResponse> issues)
    {
        return new LearningAdminCardIssueResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            AvailableModes = LearningAdminHelper.BuildAvailableModes(card, issues),
            Issues = issues,
        };
    }
}
