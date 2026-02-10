using Application.DTOs.Card;
using Application.DTOs.VocabularyCard;
using Domain.Entities;

namespace Application.Mappings;

public static class VocabularyCardMappings
{
    public static PreviewCardDTO ToPreviewDTO(this VocabularyCard card)
    {
        return new PreviewCardDTO()
        {
            Id = card.Id,
            Meaning = card.Meaning,
            Term = card.Term
        };
    }

    public static VocabularyCardDTO ToDTO(this VocabularyCard card)
    {
        return new VocabularyCardDTO()
        {
            Id = card.Id,
            Term = card.Term,
            Meaning = card.Meaning,
            DeckId = card.DeckId,
            Examples = card.ExampleSentences.Select(e => e.ToDTO()).ToList()
        };
    }
}
