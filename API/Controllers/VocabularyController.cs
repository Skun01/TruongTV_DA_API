using Application.Common;
using Application.DTOs.Vocabulary;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/vocabulary")]
public class VocabularyController : BaseController
{
    private readonly IVocabularyDetailService _vocabularyDetailService;

    public VocabularyController(IVocabularyDetailService vocabularyDetailService)
    {
        _vocabularyDetailService = vocabularyDetailService;
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet]
    public async Task<ApiResponse<List<VocabularyListItemResponse>>> Search([FromQuery] VocabularySearchQuery query)
    {
        var userId = GetCurrentUserId();

        var (items, meta) = await _vocabularyDetailService.SearchAsync(query, userId);

        return ApiResponse<List<VocabularyListItemResponse>>.SuccessResponse(items, meta);
    }

    [AllowAnonymous]
    [HttpGet("{cardId}")]
    public async Task<ApiResponse<VocabularyDetailResponse>> GetDetail([FromRoute] string cardId)
    {
        var userId = GetCurrentUserIdOrNull();
        var result = await _vocabularyDetailService.GetDetailAsync(cardId, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost]
    public async Task<ApiResponse<VocabularyDetailResponse>> Create([FromBody] CreateVocabularyCardRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.CreateAsync(request, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPatch("{cardId}")]
    public async Task<ApiResponse<VocabularyDetailResponse>> Update([FromRoute] string cardId, [FromBody] UpdateVocabularyCardRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.UpdateAsync(cardId, request, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpDelete("{cardId}")]
    public async Task<ApiResponse<bool>> SoftDelete([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.SoftDeleteAsync(cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }
}
