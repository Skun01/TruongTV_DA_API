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
}
