using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.ExampleSentence;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("examples")]
[Authorize]
public class ExampleSentenceController : BaseController
{
    private readonly IExampleSentenceService _service;
    public ExampleSentenceController(IExampleSentenceService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tạo ví dụ cho card
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResponse<bool>> Create([FromBody] CreateCardExampleRequest request)
    {
        var result = await HandleException(_service.CreateExampleSentence(request, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Cập nhật ví dụ
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}")]
    public async Task<ApiResponse<bool>> Update([FromBody] UpdateCardExampleRequest request, [FromRoute] string id)
    {
        var result = await HandleException(_service.UpdateExampleAsync(request, id));

        return result;
    }
}
