using Application.Common;
using Application.DTOs.VocabularyCard;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("vocabularies")]
[Authorize]
public class VocabularyCardController : BaseController
{
    private readonly IVocabularyCardService _service;
    public VocabularyCardController(IVocabularyCardService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tạo một card cho một deck
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResponse<bool>> Create([FromBody] CreateVocabularyRequest request)
    {
        var result = await HandleException(_service.CreateVocabularyCardAsync(request, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Lấy danh sách các card của một deck vocabulary
    /// </summary>
    /// <param name="deckId"></param>
    /// <returns></returns>
    [HttpGet("deck/{deckId}")]
    public async Task<ApiResponse<IEnumerable<VocabularyCardDTO>>> GetCardListByDeckId([FromRoute] string deckId)
    {
        var result = await HandleException(_service.GetVocabularyListByDeckIdAsync(deckId));

        return result;
    }

    /// <summary>
    /// Lấy chi tiết một card theo id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<VocabularyCardDTO>> GetCardById([FromRoute] string id)
    {
        var result = await HandleException(_service.GetCardByIdAsync(id, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Cập nhật card từ vựng theo id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update([FromRoute] string id, [FromBody] UpdateVocabularyCardRequest request)
    {
        var result = await HandleException(_service.UpdateCardByIdAsync(request, id, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Xoó card từ vựng theo id
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
