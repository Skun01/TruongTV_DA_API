using Application.Common;
using Application.DTOs.Cards;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Enums;

namespace Application.Services;

public class CardService : ICardService
{
    private readonly IUnitOfWork _unitOfWork;

    public CardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<CardListItemResponse> Items, MetaData Meta)> SearchAsync(CardSearchQuery query)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var cardTypeEnum = EnumParsingHelper.ParseNullable<CardType>(query.CardType);
        var levelEnum = EnumParsingHelper.ParseNullable<JlptLevel>(query.Level);

        var (items, total) = await _unitOfWork.Cards.SearchCardsAsync(
            cardTypeEnum,
            query.Q,
            levelEnum,
            page,
            pageSize);

        var mappedItems = items.Select(item => item.ToCardListItemResponse()).ToList();

        var meta = new MetaData
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
        };

        return (mappedItems, meta);
    }
}
