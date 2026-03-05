using Application.Common;
using Application.DTOs.Review;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("review")]
[Authorize]
public class ReviewController : BaseController
{
    private readonly IReviewService _service;
    public ReviewController(IReviewService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy session review cho một deck (các card đến lúc cần ôn tập)
    /// </summary>
    /// <param name="deckId"></param>
    /// <returns></returns>
    [HttpGet("deck/{deckId}")]
    public async Task<ApiResponse<ReviewSessionDTO>> GetReviewSession([FromRoute] string deckId)
    {
        var result = await HandleException(_service.GetReviewSessionAsync(deckId, GetCurrentUserId()));

        return result;
    }

    /// <summary>
    /// Nộp câu trả lời review
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("submit")]
    public async Task<ApiResponse<ReviewResultDTO>> SubmitReview([FromBody] SubmitReviewRequest request)
    {
        var result = await HandleException(_service.SubmitReviewAsync(request, GetCurrentUserId()));

        return result;
    }
}
