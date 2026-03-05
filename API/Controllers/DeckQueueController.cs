using Application.Common;
using Application.DTOs.DeckQueue;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("queue")]
[Authorize]
public class DeckQueueController : BaseController
{
    private readonly IDeckQueueService _service;
    public DeckQueueController(IDeckQueueService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách deck trong hàng đợi học
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResponse<IEnumerable<DeckQueueDTO>>> GetQueue()
    {
        var result = await HandleException(_service.GetQueueAsync(GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Thêm deck vào hàng đợi học
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResponse<bool>> AddToQueue([FromBody] AddToQueueRequest request)
    {
        var result = await HandleException(_service.AddToQueueAsync(request.DeckId, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Xóa deck khỏi hàng đợi học
    /// </summary>
    /// <param name="deckId"></param>
    /// <returns></returns>
    [HttpDelete("{deckId}")]
    public async Task<ApiResponse<bool>> RemoveFromQueue([FromRoute] string deckId)
    {
        var result = await HandleException(_service.RemoveFromQueueAsync(deckId, GetCurrentUserId()));

        return result;
    }
}
