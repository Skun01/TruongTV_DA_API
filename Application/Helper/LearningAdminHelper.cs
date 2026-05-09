using Application.DTOs.LearningAdmin;
using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class LearningAdminHelper
{
    public static List<LearningAdminCardIssueItemResponse> BuildIssues(Card card)
    {
        var issues = new List<LearningAdminCardIssueItemResponse>();
        var cardSentences = card.CardSentences
            .Where(cs => cs.Sentence != null)
            .OrderBy(cs => cs.Position)
            .ToList();

        if (string.IsNullOrWhiteSpace(card.Summary))
        {
            issues.Add(CreateIssue(
                LearningIssueType.MissingSummary,
                "Card summary is required for flashcard and multiple-choice."));
        }

        if (card.CardType != CardType.Kanji && cardSentences.Count == 0)
        {
            issues.Add(CreateIssue(
                LearningIssueType.MissingSentence,
                "Card needs at least one attached sentence for fill-in practice."));
        }

        var hasDuplicatePosition = cardSentences
            .GroupBy(cs => cs.Position)
            .Any(group => group.Key <= 0 || group.Count() > 1);

        if (hasDuplicatePosition)
        {
            issues.Add(CreateIssue(
                LearningIssueType.DuplicateSentencePosition,
                "Sentence positions must be unique and greater than zero."));
        }

        foreach (var cardSentence in cardSentences)
        {
            if (!string.IsNullOrWhiteSpace(cardSentence.BlankWord)
                && !cardSentence.Sentence!.Text.Contains(cardSentence.BlankWord, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(CreateIssue(
                    LearningIssueType.BlankWordNotFoundInSentence,
                    "blankWord does not appear in the attached sentence text.",
                    cardSentence.SentenceId));
            }

            if (card.CardType != CardType.Kanji
                && cardSentence.AnswerList.Count == 0
                && string.IsNullOrWhiteSpace(cardSentence.BlankWord))
            {
                issues.Add(CreateIssue(
                    LearningIssueType.MissingAnswerList,
                    "Sentence should define answerList or blankWord for explicit fill-in evaluation.",
                    cardSentence.SentenceId));
            }
        }

        return issues;
    }

    public static bool IsFillInBlankReady(Card card, List<LearningAdminCardIssueItemResponse> issues)
    {
        return LearningModeEligibilityHelper.IsFillInBlankReady(card)
            && !issues.Any(issue =>
                issue.Type == LearningIssueType.MissingSentence.ToString()
                || issue.Type == LearningIssueType.MissingAnswerList.ToString()
                || issue.Type == LearningIssueType.BlankWordNotFoundInSentence.ToString()
                || issue.Type == LearningIssueType.DuplicateSentencePosition.ToString());
    }

    public static bool IsMultipleChoiceReady(List<LearningAdminCardIssueItemResponse> issues)
    {
        return !issues.Any(issue => issue.Type == LearningIssueType.MissingSummary.ToString());
    }

    public static bool IsFlashcardReady(List<LearningAdminCardIssueItemResponse> issues)
    {
        return !issues.Any(issue => issue.Type == LearningIssueType.MissingSummary.ToString());
    }

    public static List<string> BuildAvailableModes(Card card, List<LearningAdminCardIssueItemResponse> issues)
    {
        var availableModes = new List<string>();

        if (IsFillInBlankReady(card, issues))
            availableModes.Add(StudyMode.FillInBlank.ToString());

        if (IsMultipleChoiceReady(issues))
            availableModes.Add(StudyMode.MultipleChoice.ToString());

        if (IsFlashcardReady(issues))
            availableModes.Add(StudyMode.Flashcard.ToString());

        return availableModes;
    }

    public static List<LearningAdminCardIssueItemResponse> FilterIssuesByMode(
        IEnumerable<LearningAdminCardIssueItemResponse> issues,
        StudyMode? mode)
    {
        var issueList = issues.ToList();
        if (!mode.HasValue)
            return issueList;

        var allowedIssueTypes = mode.Value switch
        {
            StudyMode.FillInBlank => new[]
            {
                LearningIssueType.MissingSentence.ToString(),
                LearningIssueType.MissingAnswerList.ToString(),
                LearningIssueType.BlankWordNotFoundInSentence.ToString(),
                LearningIssueType.DuplicateSentencePosition.ToString(),
            },
            StudyMode.MultipleChoice => new[]
            {
                LearningIssueType.MissingSummary.ToString(),
                LearningIssueType.UnsupportedCardTypeForMode.ToString(),
            },
            StudyMode.Flashcard => new[]
            {
                LearningIssueType.MissingSummary.ToString(),
            },
            _ => Array.Empty<string>(),
        };

        return issueList
            .Where(issue => allowedIssueTypes.Contains(issue.Type, StringComparer.Ordinal))
            .ToList();
    }

    private static LearningAdminCardIssueItemResponse CreateIssue(
        LearningIssueType issueType,
        string message,
        string? sentenceId = null)
    {
        return new LearningAdminCardIssueItemResponse
        {
            Type = issueType.ToString(),
            Message = message,
            SentenceId = sentenceId,
        };
    }
}
