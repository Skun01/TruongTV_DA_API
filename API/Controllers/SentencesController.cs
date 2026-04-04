using Application.Common;
using Application.DTOs.Sentences;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
[Route("api/sentences")]
public class SentencesController : BaseController
{
    private readonly ISentenceService _sentenceService;

    public SentencesController(ISentenceService sentenceService)
    {
        _sentenceService = sentenceService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<SentenceResponse>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (items, meta) = await _sentenceService.SearchAsync(q, level, page, pageSize);
        return ApiResponse<List<SentenceResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<SentenceResponse>> GetById([FromRoute] string id)
    {
        var item = await _sentenceService.GetByIdAsync(id);
        return ApiResponse<SentenceResponse>.SuccessResponse(item);
    }

    [HttpPost]
    public async Task<ApiResponse<SentenceResponse>> Create([FromBody] CreateSentenceRequest request)
    {
        var item = await _sentenceService.CreateAsync(request);
        return ApiResponse<SentenceResponse>.SuccessResponse(item);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<SentenceResponse>> Update([FromRoute] string id, [FromBody] UpdateSentenceRequest request)
    {
        var item = await _sentenceService.UpdateAsync(id, request);
        return ApiResponse<SentenceResponse>.SuccessResponse(item);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string id)
    {
        var deleted = await _sentenceService.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted);
    }
}
