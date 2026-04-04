using Application.DTOs.CardNotes;
using Application.DTOs.Vocabulary;
using Domain.Entities;

namespace Application.Mappings;

public static class VocabularyMappings
{
    public static VocabularyDetailResponse ToDetailResponse(this Card card, List<UserCardNote> notes, string? currentUserId)
    {
        return new VocabularyDetailResponse
        {
            Id = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            Writing = card.VocabularyDetail?.Writing ?? string.Empty,
            Reading = card.VocabularyDetail?.Reading,
            PitchPattern = ParsePitchPattern(card.VocabularyDetail?.PitchAccent),
            AudioUrl = card.VocabularyDetail?.AudioUrl,
            WordType = card.VocabularyDetail?.WordType?.ToString(),
            Meanings = card.VocabularyDetail?.Meanings
                .Select(m => new VocabularyMeaningResponse
                {
                    PartOfSpeech = m.PartOfSpeech.ToString(),
                    Definitions = m.Definitions,
                })
                .ToList() ?? new List<VocabularyMeaningResponse>(),
            Synonyms = card.VocabularyDetail?.Synonyms ?? new List<string>(),
            Antonyms = card.VocabularyDetail?.Antonyms ?? new List<string>(),
            RelatedPhrases = card.VocabularyDetail?.RelatedPhrases ?? new List<string>(),
            Sentences = card.CardSentences
                .Select(cs => cs.Sentence)
                .Where(s => s != null)
                .Select(s => new VocabularySentenceResponse
                {
                    Id = s!.Id,
                    Text = s.Text,
                    Meaning = s.Meaning,
                    AudioUrl = s.AudioUrl,
                    Level = s.Level?.ToString(),
                })
                .ToList(),
            UserNotes = notes.Select(n => n.ToCardNoteResponse(currentUserId ?? string.Empty)).ToList(),
        };
    }

    public static VocabularyListItemResponse ToListItemResponse(this Card card)
    {
        return new VocabularyListItemResponse
        {
            Id = card.Id,
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags,
            Status = card.Status.ToString(),
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            Writing = card.VocabularyDetail?.Writing ?? string.Empty,
            Reading = card.VocabularyDetail?.Reading,
            WordType = card.VocabularyDetail?.WordType?.ToString(),
        };
    }

    private static List<int>? ParsePitchPattern(string? pitchAccent)
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
