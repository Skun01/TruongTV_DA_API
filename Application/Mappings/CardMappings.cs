using Application.DTOs.Cards;
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
        };
    }
}
