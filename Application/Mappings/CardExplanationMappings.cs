using Application.DTOs.Cards;
using Domain.Entities;

namespace Application.Mappings;

public static class CardExplanationMappings
{
    public static CardExplanationResponse ToExplanationResponse(
        this CardExplanationContent content,
        Card card,
        string model)
    {
        return new CardExplanationResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Level = card.Level?.ToString(),
            Answer = content.Answer,
            Model = model,
        };
    }
}
