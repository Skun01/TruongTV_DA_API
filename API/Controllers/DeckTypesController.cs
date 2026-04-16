using Application.Common;
using Application.DTOs.Decks;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/deck-types")]
public class DeckTypesController : BaseController
{
    private readonly IDeckTypeService _deckTypeService;

    public DeckTypesController(IDeckTypeService deckTypeService)
    {
        _deckTypeService = deckTypeService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponse<List<DeckTypeResponse>>> GetAll()
    {
        var result = await _deckTypeService.GetAllAsync();
        return ApiResponse<List<DeckTypeResponse>>.SuccessResponse(result);
    }
}
