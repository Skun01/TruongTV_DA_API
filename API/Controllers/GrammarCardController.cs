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
    public async Task<ApiResponse<bool>> CreateCard([FromBody] CreateGrammarCardRequest request)
    {
        var result = await HandleException(_service.CreateGrammarCardAsync(request, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Lấy danh chi tiết các grammar card theo deck id
    /// </summary>
    /// <param name="deckId"></param>
    /// <returns></returns>
    [HttpGet("deck/{deckId}")]
    public async Task<ApiResponse<IEnumerable<GrammarCardDTO>>> Get([FromRoute] string deckId)
    {
        var result = await HandleException(_service.GetGrammarListByDeckIdAsync(deckId, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Lấy thông tin chi tiết 1 grammar card theo id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<GrammarCardDTO>> GetCardById([FromRoute] string id)
    {
        var result = await HandleException(_service.GetCardByIdAsync(id));

        return result;
    }

    /// <summary>
    /// Cập nhật grammar card theo id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update([FromRoute] string id, [FromBody] UpdateGrammarCardRequest request)
    {
        var result = await HandleException(_service.UpdateCardByIdAsync(request, id, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Xóa card theo id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string id)
    {
        var result = await HandleException(_service.DeleteByIdAsync(id, GetCurrentUserId()));

        return result;
    }
}
