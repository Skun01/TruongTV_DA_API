using Application.DTOs.Sentences;
using Domain.Entities;

namespace Application.Mappings;

public static class SentenceMappings
{
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
