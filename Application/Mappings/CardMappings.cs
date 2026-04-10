using Application.DTOs.Cards;
using Application.DTOs.Grammar;
using Application.DTOs.Vocabulary;
using Domain.Entities;

namespace Application.Mappings;

public static class CardMappings
{
    public static CardListItemResponse ToCardListItemResponse(this Card card)
    {
        return new CardListItemResponse
        {
            Id = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            Level = card.Level?.ToString(),
            AlternateForms = new List<string>(),
        };
    }

    public static CardListItemResponse ToCardListItemResponse(this VocabularyListItemResponse item)
    {
        return new CardListItemResponse
        {
            Id = item.Id,
            CardType = "Vocab",
            Title = item.Title,
            Summary = item.Summary,
            Level = item.Level,
            AlternateForms = new List<string>(),
        };
    }

    public static CardListItemResponse ToCardListItemResponse(this GrammarListItemResponse item)
    {
        return new CardListItemResponse
        {
            Id = item.Id,
            CardType = "Grammar",
            Title = item.Title,
            Summary = item.Summary,
            Level = item.Level,
            AlternateForms = item.AlternateForms,
        };
    }
}
