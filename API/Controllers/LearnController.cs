using Application.Common;
using Application.DTOs.Deck;
using Application.DTOs.Learn;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("learn")]
[Authorize]
public class LearnController : BaseController
{
    private readonly ILearnService _service;
    public LearnController(ILearnService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy batch card để học từ một deck
    /// </summary>
    /// <param name="deckId"></param>
    /// <returns></returns>
    [HttpGet("deck/{deckId}")]
    public async Task<ApiResponse<LearnBatchDTO>> GetLearnBatch([FromRoute] string deckId)
    {
        var result = await HandleException(_service.GetLearnBatchAsync(deckId, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Đánh dấu card đã học hoặc mastered
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("mark")]
    public async Task<ApiResponse<bool>> MarkCardLearned([FromBody] MarkCardRequest request)
    {
        var result = await HandleException(_service.MarkCardLearnedAsync(request, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Lấy tiến độ daily goal
    /// </summary>
    /// <returns></returns>
    [HttpGet("progress")]
    public async Task<ApiResponse<DailyProgressDTO>> GetDailyProgress()
    {
        var result = await HandleException(_service.GetDailyProgressAsync(GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Lấy thống kê tiến độ của một deck
    /// </summary>
    /// <param name="deckId"></param>
    /// <returns></returns>
    [HttpGet("deck/{deckId}/stats")]
    public async Task<ApiResponse<DeckProgressDTO>> GetDeckProgress([FromRoute] string deckId)
    {
        var result = await HandleException(_service.GetDeckProgressAsync(deckId, GetCurrentUserId()));

        return result;
    }
}
