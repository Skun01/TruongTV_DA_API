using Application.Common;
using Application.DTOs.Cards;

namespace Application.IServices;

public interface ICardService
{
    Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchAsync(CardSearchQuery query);
}
