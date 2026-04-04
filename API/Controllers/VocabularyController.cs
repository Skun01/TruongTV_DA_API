using Application.Common;
using Application.DTOs.Vocabulary;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/vocabulary")]
public class VocabularyController : BaseController
{
    private readonly IVocabularyDetailService _vocabularyDetailService;

    public VocabularyController(IVocabularyDetailService vocabularyDetailService)
    {
        _vocabularyDetailService = vocabularyDetailService;
    }

    [HttpGet("{cardId}")]
    public async Task<ApiResponse<VocabularyDetailResponse>> GetDetail([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.GetDetailAsync(cardId, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }
}
