using Application.DTOs.Card;
using Application.DTOs.GrammarCard;
using Domain.Entities;

namespace Application.Mappings;

public static class GrammarCardMappings
{
    public static PreviewCardDTO ToPreviewDTO(this GrammarCard card)
    {
        return new PreviewCardDTO()
        {
            Id = card.Id,
            Term = card.Term,
            Meaning = card.Meaning
        };
    }

    public static GrammarCardDTO ToDTO(this GrammarCard card, string deckId = "")
    {
        return new GrammarCardDTO()
        {
            Id = card.Id,
            Term = card.Term,
            Meaning = card.Meaning,
            Structure = card.Structure,
            Explanation = card.Explanation,
            Caution = card.Caution,
            DeckId = card.Deck?.Id ?? deckId,
            Examples = card.ExampleSentences.Select(e => e.ToDTO()).ToList()
        };
    }
}
