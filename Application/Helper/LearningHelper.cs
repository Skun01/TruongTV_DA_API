using Domain.Entities;
using Domain.Enums;

namespace Application.Helper;

public static class LearningHelper
{
    public static SrsLevel IncreaseLevel(SrsLevel level, int step = 1)
    {
        var nextValue = Math.Min((int)level + Math.Max(step, 1), (int)SrsLevel.Level5);
        return (SrsLevel)nextValue;
    }

    public static SrsLevel DecreaseLevel(SrsLevel level, int step = 1)
    {
        var nextValue = Math.Max((int)level - Math.Max(step, 1), (int)SrsLevel.Level1);
        return (SrsLevel)nextValue;
    }

    public static DateTime ResolveNextReviewAt(SrsLevel level, DateTime nowUtc)
    {
        return level switch
        {
            SrsLevel.Level1 => nowUtc.AddHours(8),
            SrsLevel.Level2 => nowUtc.AddDays(1),
            SrsLevel.Level3 => nowUtc.AddDays(3),
            SrsLevel.Level4 => nowUtc.AddDays(7),
            SrsLevel.Level5 => nowUtc.AddDays(14),
            _ => nowUtc.AddHours(8),
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
