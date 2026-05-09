using Application.Common;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class LearningHelper
{
    public static SrsLevel IncreaseLevel(SrsLevel level, int step = 1)
    {
        var nextValue = Math.Min((int)level + Math.Max(step, 1), (int)SrsLevel.level_12);
        return (SrsLevel)nextValue;
    }

    public static SrsLevel DecreaseLevel(SrsLevel level, int step = 1)
    {
        var nextValue = Math.Max((int)level - Math.Max(step, 1), (int)SrsLevel.level_1);
        return (SrsLevel)nextValue;
    }

    public static DateTime ResolveNextReviewAt(SrsLevel level, DateTime nowUtc)
    {
        return level switch
        {
            SrsLevel.level_1 => nowUtc.AddHours(4),
            SrsLevel.level_2 => nowUtc.AddHours(8),
            SrsLevel.level_3 => nowUtc.AddHours(23),
            SrsLevel.level_4 => nowUtc.AddDays(2),
            SrsLevel.level_5 => nowUtc.AddDays(4),
            SrsLevel.level_6 => nowUtc.AddDays(8),
            SrsLevel.level_7 => nowUtc.AddDays(14),
            SrsLevel.level_8 => nowUtc.AddMonths(1),
            SrsLevel.level_9 => nowUtc.AddMonths(2),
            SrsLevel.level_10 => nowUtc.AddMonths(4),
            SrsLevel.level_11 => nowUtc.AddMonths(8),
            SrsLevel.level_12 => nowUtc.AddYears(100),
            _ => nowUtc.AddHours(4),
        };
    }

    public static bool IsMastered(UserCardProgress progress)
    {
        return progress.IsMastered || progress.SrsLevel == SrsLevel.level_12;
    }

    public static bool IsDue(UserCardProgress progress, DateTime nowUtc)
    {
        return !IsMastered(progress) && progress.NextReviewAt <= nowUtc;
    }

    public static double CalculateAccuracy(int correctCount, int totalAttempts)
    {
        return totalAttempts == 0 ? 0 : Math.Round((double)correctCount / totalAttempts * 100, 2);
    }

    public static double CalculateAverageSrsLevel(IEnumerable<UserCardProgress> progresses)
    {
        var progressList = progresses.ToList();
        return progressList.Count == 0 ? 0 : Math.Round(progressList.Average(x => (int)x.SrsLevel) + 1, 2);
    }

    public static double CalculateAverageConsecutiveCorrect(IEnumerable<UserCardProgress> progresses)
    {
        var progressList = progresses.ToList();
        return progressList.Count == 0 ? 0 : Math.Round(progressList.Average(x => x.ConsecutiveCorrect), 2);
    }

    public static List<string> BuildFallbackAnswers(Card card)
    {
        var answers = new List<string>();

        switch (card.CardType)
        {
            case CardType.Vocab:
                if (card.VocabularyDetail != null)
                {
                    answers.Add(card.VocabularyDetail.Writing);
                    if (!string.IsNullOrWhiteSpace(card.VocabularyDetail.Reading))
                        answers.Add(card.VocabularyDetail.Reading);
                }
                answers.Add(card.Title);
                break;
            case CardType.Grammar:
                answers.Add(card.Title);
                if (card.GrammarDetail != null)
                    answers.AddRange(card.GrammarDetail.AlternateForms);
                break;
            case CardType.Kanji:
                if (card.KanjiDetail != null)
                {
                    answers.Add(card.KanjiDetail.Kanji);
                    answers.Add(card.KanjiDetail.MeaningVi);
                    if (!string.IsNullOrWhiteSpace(card.KanjiDetail.HanViet))
                        answers.Add(card.KanjiDetail.HanViet);
                }
                answers.Add(card.Title);
                break;
        }

        return StringHelper.NormalizeAnswerList(answers);
    }

    public static string ReplaceFirstBlank(string text, string blankValue)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "____";

        if (!string.IsNullOrWhiteSpace(blankValue))
        {
            var index = text.IndexOf(blankValue, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return string.Concat(text.AsSpan(0, index), "____", text.AsSpan(index + blankValue.Length));
        }

        return $"{text} (____)";
    }

    public static List<string> DistinctByRandomOrder(IEnumerable<string> items, Random random)
    {
        return items
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => random.Next())
            .ToList();
    }

    public static StudyMode ParseStudyMode(string mode)
    {
        try
        {
            return EnumParsingHelper.ParseRequired<StudyMode>(mode);
        }
        catch (ApplicationException)
        {
            throw new AppException(MessageConstants.LearningMessage.INVALID_MODE, 400);
        }
    }

    public static FlashcardReviewResult ParseFlashcardResult(string? result)
    {
        try
        {
            return EnumParsingHelper.ParseRequired<FlashcardReviewResult>(result ?? string.Empty);
        }
        catch (ApplicationException)
        {
            throw new AppException(MessageConstants.LearningMessage.INVALID_SUBMISSION, 400);
        }
    }

    public static TEnum ResolveEnumSetting<TEnum>(string? requestValue, TEnum? userValue, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(requestValue))
            return EnumParsingHelper.ParseRequired<TEnum>(requestValue);

        return userValue ?? fallback;
    }

    public static TEnum ResolveEnumSetting<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        try
        {
            return EnumParsingHelper.ParseRequired<TEnum>(value);
        }
        catch (ApplicationException)
        {
            throw new AppException(MessageConstants.CommonMessage.INVALID, 400);
        }
    }

    public static string ResolveFlashcardContent(Card card, FlashcardContentType contentType)
    {
        return contentType switch
        {
            FlashcardContentType.Summary => card.Summary,
            _ => card.Title,
        };
    }

    public static List<string> NormalizeRequestedIds(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static int NormalizeLimit(int? value, int defaultValue, int maxValue)
    {
        if (!value.HasValue || value.Value <= 0)
            return defaultValue;

        return Math.Min(value.Value, maxValue);
    }

    public static void ApplySentenceConfig(CardSentence link, int position, string? blankWord, string? hint, List<string> answerList)
    {
        link.Position = position;
        link.BlankWord = StringHelper.NormalizeOptional(blankWord);
        link.Hint = StringHelper.NormalizeOptional(hint);
        link.AnswerList = StringHelper.NormalizeAnswerList(answerList, blankWord);
    }
}
