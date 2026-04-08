using Application.Common;
using Application.DTOs.Cards;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/cards")]
public class CardsController : BaseController
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [HttpGet("search")]
    public async Task<ApiResponse<List<CardListItemResponse>>> Search([FromQuery] CardSearchQuery query)
    {
        var (items, meta) = await _cardService.SearchAsync(query);

        return ApiResponse<List<CardListItemResponse>>.SuccessResponse(items, meta);
    }
}
