using Application.DTOs.Sentences;
using Domain.Entities;

namespace Application.Mappings;

public static class SentenceMappings
{
    public static ImportSentenceItemRequest ToImportItem(this Sentence sentence)
    {
        return new ImportSentenceItemRequest
        {
            Text = sentence.Text,
            Meaning = sentence.Meaning,
            Level = sentence.Level?.ToString(),
        };
    }

    public static CreateSentenceRequest ToCreateRequest(this ImportSentenceItemRequest item)
    {
        return new CreateSentenceRequest
        {
            Text = item.Text,
            Meaning = item.Meaning,
            Level = item.Level,
        };
    }

    public static SentenceResponse ToResponse(this Sentence sentence)
    {
        return new SentenceResponse
        {
            Id = sentence.Id,
            Text = sentence.Text,
            Meaning = sentence.Meaning,
            AudioUrl = sentence.AudioUrl,
            Level = sentence.Level?.ToString(),
            CreatedAt = sentence.CreatedAt,
            UpdatedAt = sentence.UpdatedAt,
        };
    }
}
