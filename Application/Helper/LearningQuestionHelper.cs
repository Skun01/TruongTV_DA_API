using Application.Common;
using Application.DTOs.Learning;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class LearningQuestionHelper
{
    public static LearningAnswerPayload BuildAnswerPayload(Card card, UserCardProgress? progress, StudySession session)
    {
        return session.Mode switch
        {
            StudyMode.FillInBlank => BuildFillInBlankPayload(card, progress),
            StudyMode.MultipleChoice => BuildMultipleChoicePayload(card, session),
            StudyMode.Flashcard => BuildFlashcardPayload(card, session),
            _ => throw new AppException(MessageConstants.LearningMessage.INVALID_MODE, 400),
        };
    }

    public static bool EvaluateSubmission(LearningAnswerPayload payload, SubmitStudyAnswerRequest request, StudyMode mode)
    {
        return mode switch
        {
            StudyMode.FillInBlank => request.Answers
                .Select(answer => answer.Trim())
                .Any(answer => payload.AcceptedAnswers.Contains(answer, StringComparer.OrdinalIgnoreCase)),
            StudyMode.MultipleChoice => payload.AcceptedAnswers.Count > 0
                && payload.AcceptedAnswers.Count == request.SelectedOptionIds.Count
                && payload.AcceptedAnswers.All(answer => request.SelectedOptionIds.Contains(answer, StringComparer.OrdinalIgnoreCase)),
            StudyMode.Flashcard => LearningHelper.ParseFlashcardResult(request.FlashcardResult) == FlashcardReviewResult.Known,
            _ => false,
        };
    }

    public static void ApplyProgress(UserCardProgress progress, StudyMode mode, SubmitStudyAnswerRequest request, bool isCorrect, DateTime now)
    {
        if (mode == StudyMode.Flashcard)
        {
            var result = LearningHelper.ParseFlashcardResult(request.FlashcardResult);
            if (result == FlashcardReviewResult.Learning)
            {
                progress.SrsLevel = SrsLevel.level_1;
                progress.ConsecutiveCorrect = 0;
            }
            else
            {
                progress.SrsLevel = LearningHelper.IncreaseLevel(progress.SrsLevel);
                progress.ConsecutiveCorrect++;
            }
        }
        else if (isCorrect)
        {
            progress.SrsLevel = LearningHelper.IncreaseLevel(progress.SrsLevel);
            progress.ConsecutiveCorrect++;
        }
        else
        {
            progress.SrsLevel = SrsLevel.level_1;
            progress.ConsecutiveCorrect = 0;
        }

        progress.IsMastered = progress.SrsLevel == SrsLevel.level_12;
        progress.NextReviewAt = LearningHelper.ResolveNextReviewAt(progress.SrsLevel, now);
    }

    public static string ResolveMultipleChoiceValue(Card card, MultipleChoiceQuestionType questionType)
    {
        return questionType == MultipleChoiceQuestionType.SummaryToTitle
            ? card.Title
            : card.Summary;
    }

    public static List<StudyQuestionOptionResponse> BuildMultipleChoiceOptions(
        List<string> acceptedAnswers,
        IEnumerable<string> distractors,
        bool shuffleOptions,
        Random random)
    {
        var options = new List<string>(acceptedAnswers);

        options.AddRange(
            LearningHelper.DistinctByRandomOrder(distractors, random)
                .Where(option => !acceptedAnswers.Contains(option, StringComparer.OrdinalIgnoreCase))
                .Take(Math.Max(4 - options.Count, 0)));

        var finalOptions = shuffleOptions
            ? LearningHelper.DistinctByRandomOrder(options, random)
            : options
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        return finalOptions
            .Select(option => new StudyQuestionOptionResponse
            {
                Id = option,
                Text = option,
            })
            .ToList();
    }

    private static LearningAnswerPayload BuildFillInBlankPayload(Card card, UserCardProgress? progress)
    {
        var selected = SelectCardSentence(card, progress);
        if (selected != null)
        {
            var acceptedAnswers = StringHelper.NormalizeAnswerList(
                selected.AnswerList,
                selected.BlankWord);

            if (acceptedAnswers.Count == 0)
                acceptedAnswers = LearningHelper.BuildFallbackAnswers(card);

            var blankValue = selected.BlankWord ?? acceptedAnswers.FirstOrDefault() ?? string.Empty;
            return new LearningAnswerPayload(
                "Điền vào chỗ trống",
                LearningHelper.ReplaceFirstBlank(selected.Sentence.Text, blankValue),
                selected.Sentence.Meaning,
                selected.Hint,
                acceptedAnswers,
                selected.SentenceId,
                null,
                null);
        }

        var fallbackAnswers = LearningHelper.BuildFallbackAnswers(card);
        return new LearningAnswerPayload(
            "Điền đáp án phù hợp cho thẻ",
            card.Summary,
            card.Title,
            null,
            fallbackAnswers,
            null,
            null,
            null);
    }

    private static LearningAnswerPayload BuildMultipleChoicePayload(Card card, StudySession session)
    {
        var question = session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
            ? card.Summary
            : card.Title;
        var answer = ResolveMultipleChoiceValue(card, session.MultipleChoiceQuestion);

        return new LearningAnswerPayload(
            session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
                ? "Chọn từ khóa đúng của nghĩa"
                : "Chọn nghĩa đúng của thẻ",
            question,
            null,
            null,
            new List<string> { answer },
            null,
            null,
            null);
    }

    private static LearningAnswerPayload BuildFlashcardPayload(Card card, StudySession session)
    {
        return new LearningAnswerPayload(
            "Xem flashcard rồi đánh dấu đang học hoặc đã biết",
            null,
            null,
            null,
            new List<string>(),
            null,
            LearningHelper.ResolveFlashcardContent(card, session.FlashcardFront),
            LearningHelper.ResolveFlashcardContent(card, session.FlashcardBack));
    }

    private static CardSentence? SelectCardSentence(Card card, UserCardProgress? progress)
    {
        var sentences = card.CardSentences
            .Where(cs => cs.Sentence != null)
            .OrderBy(cs => cs.Position)
            .ToList();

        if (sentences.Count == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(progress?.LastSentenceId))
        {
            var nextSentence = sentences.FirstOrDefault(cs => cs.SentenceId != progress.LastSentenceId);
            if (nextSentence != null)
                return nextSentence;
        }

        return sentences.First();
    }
}
