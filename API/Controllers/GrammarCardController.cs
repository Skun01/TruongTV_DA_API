using Application.Common;
using Application.DTOs.GrammarCard;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("grammars")]
[Authorize]
public class GrammarCardController : BaseController
{
    private IGrammarCardService _service;
    public GrammarCardController(IGrammarCardService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tạo grammar card cho một grammar deck
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResponse<bool>> CreateCard(CreateGrammarCardRequest request)
    {
        var result = await HandleException(_service.CreateGrammarCardAsync(request, GetCurrentUserId()));

        return result;
    }
}
