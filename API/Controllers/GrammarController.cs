using Application.Common;
using Application.DTOs.Grammar;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/grammar")]
public class GrammarController : BaseController
{
    private readonly IGrammarService _grammarService;

    public GrammarController(IGrammarService grammarService)
    {
        _grammarService = grammarService;
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet]
    public async Task<ApiResponse<List<GrammarListItemResponse>>> Search([FromQuery] GrammarSearchQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _grammarService.SearchAsync(query, userId);
        return ApiResponse<List<GrammarListItemResponse>>.SuccessResponse(items, meta);
    }

    [AllowAnonymous]
    [HttpGet("{cardId}")]
    public async Task<ApiResponse<GrammarDetailResponse>> GetDetail([FromRoute] string cardId)
    {
        var userId = GetCurrentUserIdOrNull();
        var item = await _grammarService.GetDetailAsync(cardId, userId);
        return ApiResponse<GrammarDetailResponse>.SuccessResponse(item);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost]
    public async Task<ApiResponse<GrammarDetailResponse>> Create([FromBody] CreateGrammarCardRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _grammarService.CreateAsync(request, userId);
        return ApiResponse<GrammarDetailResponse>.SuccessResponse(item);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPatch("{cardId}")]
    public async Task<ApiResponse<GrammarDetailResponse>> Update([FromRoute] string cardId, [FromBody] UpdateGrammarCardRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _grammarService.UpdateAsync(cardId, request, userId);
        return ApiResponse<GrammarDetailResponse>.SuccessResponse(item);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpDelete("{cardId}")]
    public async Task<ApiResponse<bool>> SoftDelete([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _grammarService.SoftDeleteAsync(cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }
}
