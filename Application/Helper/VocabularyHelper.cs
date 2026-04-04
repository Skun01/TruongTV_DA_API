using Application.DTOs.Vocabulary;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Helper;

public static class VocabularyHelper
{
    public static List<MeaningItem> MapMeaningItems(List<VocabularyMeaningRequest> meanings)
    {
        return meanings
            .Select(m => new MeaningItem
            {
                PartOfSpeech = EnumParsingHelper.ParseRequired<PartOfSpeech>(m.PartOfSpeech),
                Definitions = StringHelper.NormalizeList(m.Definitions),
            })
            .ToList();
    }

    public static string? SerializePitchPattern(List<int>? pitchPattern)
    {
        if (pitchPattern == null || pitchPattern.Count == 0)
            return null;

        return string.Join(",", pitchPattern);
    }

    public static List<int>? ParsePitchPattern(string? pitchAccent)
    {
        if (string.IsNullOrWhiteSpace(pitchAccent))
            return null;

        var normalized = pitchAccent
            .Replace("[", string.Empty)
            .Replace("]", string.Empty)
            .Trim();

        var parsed = normalized
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => int.TryParse(value, out var number) ? number : (int?)null)
            .Where(number => number.HasValue)
            .Select(number => number!.Value)
            .ToList();

        return parsed.Count == 0 ? null : parsed;
    }
}
