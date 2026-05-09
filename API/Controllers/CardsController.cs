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

    /// <summary>
    /// Tìm kiếm card tổng hợp (Vocabulary + Grammar) cho frontend user. Chỉ trả card Published.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ApiResponse<List<CardListItemResponse>>> Search([FromQuery] CardSearchQuery query)
    {
        var (items, meta) = await _cardService.SearchAsync(query);

        return ApiResponse<List<CardListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Giải thích card bằng AI cho người học hiện tại.
    /// </summary>
    [HttpPost("{cardId}/explain")]
    public async Task<ApiResponse<CardExplanationResponse>> Explain(
        [FromRoute] string cardId,
        [FromBody] ExplainCardRequest? request)
    {
        var result = await _cardService.ExplainAsync(cardId, request ?? new ExplainCardRequest());

        return ApiResponse<CardExplanationResponse>.SuccessResponse(result);
    }
}
