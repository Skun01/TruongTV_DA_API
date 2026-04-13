using Application.Common;
using Application.DTOs.Kanji;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace API.Controllers;

[ApiController]
[Route("api/kanji")]
public class KanjiController : BaseController
{
    private readonly IKanjiService _kanjiService;

    public KanjiController(IKanjiService kanjiService)
    {
        _kanjiService = kanjiService;
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet]
    public async Task<ApiResponse<List<KanjiListItemResponse>>> Search([FromQuery] KanjiSearchQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _kanjiService.SearchAsync(query, userId);
        return ApiResponse<List<KanjiListItemResponse>>.SuccessResponse(items, meta);
    }

    [AllowAnonymous]
    [HttpGet("{cardId}")]
    public async Task<ApiResponse<KanjiDetailResponse>> GetDetail([FromRoute] string cardId)
    {
        var userId = GetCurrentUserIdOrNull();
        var item = await _kanjiService.GetDetailAsync(cardId, userId);
        return ApiResponse<KanjiDetailResponse>.SuccessResponse(item);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet("import-template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _kanjiService.GetImportTemplateAsync();
        return CreateJsonFileResult(result, "kanji-import-template.json");
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] KanjiExportQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _kanjiService.ExportAsync(query, userId);
        return CreateJsonFileResult(result, $"kanji-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("import/preview")]
    public async Task<ApiResponse<KanjiImportPreviewResponse>> PreviewImport([FromBody] ImportKanjiRequest request)
    {
        var result = await _kanjiService.PreviewImportAsync(request);
        return ApiResponse<KanjiImportPreviewResponse>.SuccessResponse(result);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("import/commit")]
    public async Task<ApiResponse<KanjiImportCommitResponse>> CommitImport([FromBody] ImportKanjiRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _kanjiService.CommitImportAsync(request, userId);
        return ApiResponse<KanjiImportCommitResponse>.SuccessResponse(result);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost]
    public async Task<ApiResponse<KanjiDetailResponse>> Create([FromBody] CreateKanjiCardRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _kanjiService.CreateAsync(request, userId);
        return ApiResponse<KanjiDetailResponse>.SuccessResponse(item);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPatch("{cardId}")]
    public async Task<ApiResponse<KanjiDetailResponse>> Update([FromRoute] string cardId, [FromBody] UpdateKanjiCardRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _kanjiService.UpdateAsync(cardId, request, userId);
        return ApiResponse<KanjiDetailResponse>.SuccessResponse(item);
    }

    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpDelete("{cardId}")]
    public async Task<ApiResponse<bool>> SoftDelete([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _kanjiService.SoftDeleteAsync(cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
    }

    private static FileContentResult CreateJsonFileResult<T>(T data, string fileName)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        });

        return new FileContentResult(Encoding.UTF8.GetBytes(json), "application/json")
        {
            FileDownloadName = fileName,
        };
    }
}
