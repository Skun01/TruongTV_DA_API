using Application.DTOs.Vocabulary;
using Application.Helper;
using Domain.Entities;

namespace Application.Mappings;

public static class VocabularyImportExportMappings
{
    public static ImportVocabularyItemRequest ToImportItem(this Card card)
    {
        var detail = card.VocabularyDetail;

        return new ImportVocabularyItemRequest
        {
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            Tags = card.Tags.ToList(),
            Status = card.Status.ToString(),
            Writing = detail?.Writing ?? string.Empty,
            Reading = detail?.Reading,
            PitchPattern = VocabularyHelper.ParsePitchPattern(detail?.PitchAccent),
            SpeakerId = detail?.SpeakerId,
            WordType = detail?.WordType?.ToString(),
            Meanings = detail?.Meanings
                .Select(m => new VocabularyMeaningRequest
                {
                    PartOfSpeech = m.PartOfSpeech.ToString(),
                    Definitions = m.Definitions.ToList(),
                })
                .ToList() ?? new List<VocabularyMeaningRequest>(),
            Synonyms = detail?.Synonyms.ToList() ?? new List<string>(),
            Antonyms = detail?.Antonyms.ToList() ?? new List<string>(),
            RelatedPhrases = detail?.RelatedPhrases.ToList() ?? new List<string>(),
            Sentences = card.CardSentences
                .Where(cs => cs.Sentence != null)
                .OrderBy(cs => cs.Position)
                .Select(cs => new VocabularySentenceUpsertRequest
                {
                    Id = cs.Sentence!.Id,
                    Position = cs.Position,
                    Text = cs.Sentence.Text,
                    Meaning = cs.Sentence.Meaning,
                    SpeakerId = cs.Sentence.SpeakerId,
                    Level = cs.Sentence.Level?.ToString(),
                    BlankWord = cs.BlankWord,
                    Hint = cs.Hint,
                    AnswerList = cs.AnswerList.ToList(),
                })
                .ToList(),
        };
    }

    public static CreateVocabularyCardRequest ToCreateRequest(this ImportVocabularyItemRequest item)
    {
        return new CreateVocabularyCardRequest
        {
            Title = item.Title,
            Summary = item.Summary,
            Level = item.Level,
            Tags = item.Tags?.ToList() ?? new List<string>(),
            Status = item.Status,
            Writing = item.Writing,
            Reading = item.Reading,
            PitchPattern = item.PitchPattern?.ToList(),
            SpeakerId = item.SpeakerId,
            WordType = item.WordType,
            Meanings = (item.Meanings ?? new List<VocabularyMeaningRequest>())
                .Select(meaning => new VocabularyMeaningRequest
                {
                    PartOfSpeech = meaning.PartOfSpeech,
                    Definitions = meaning.Definitions?.ToList() ?? new List<string>(),
                })
                .ToList(),
            Synonyms = item.Synonyms?.ToList() ?? new List<string>(),
            Antonyms = item.Antonyms?.ToList() ?? new List<string>(),
            RelatedPhrases = item.RelatedPhrases?.ToList() ?? new List<string>(),
            Sentences = (item.Sentences ?? new List<VocabularySentenceUpsertRequest>())
                .Select(sentence => new VocabularySentenceUpsertRequest
                {
                    Id = sentence.Id,
                    Position = sentence.Position,
                    Text = sentence.Text,
                    Meaning = sentence.Meaning,
                    SpeakerId = sentence.SpeakerId,
                    Level = sentence.Level,
                    BlankWord = sentence.BlankWord,
                    Hint = sentence.Hint,
                    AnswerList = sentence.AnswerList.ToList(),
                })
                .ToList(),
        };
    }

}
