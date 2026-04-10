using Application.Common;
using Application.DTOs.Sentences;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

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

    /// <summary>
    /// Tìm kiếm danh sách câu ví dụ có phân trang.
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<SentenceResponse>>> Search([FromQuery] SentenceSearchQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _sentenceService.SearchAsync(query, userId);
        return ApiResponse<List<SentenceResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("import-template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _sentenceService.GetImportTemplateAsync();
        return CreateJsonFileResult(result, "sentence-import-template.json");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] SentenceExportQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _sentenceService.ExportAsync(query, userId);
        return CreateJsonFileResult(result, $"sentence-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    [HttpPost("import/preview")]
    public async Task<ApiResponse<SentenceImportPreviewResponse>> PreviewImport([FromBody] ImportSentenceRequest request)
    {
        var result = await _sentenceService.PreviewImportAsync(request);
        return ApiResponse<SentenceImportPreviewResponse>.SuccessResponse(result);
    }

    [HttpPost("import/commit")]
    public async Task<ApiResponse<SentenceImportCommitResponse>> CommitImport([FromBody] ImportSentenceRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _sentenceService.CommitImportAsync(request, userId);
        return ApiResponse<SentenceImportCommitResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy chi tiết câu ví dụ theo id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<SentenceResponse>> GetById([FromRoute] string id)
    {
        var item = await _sentenceService.GetByIdAsync(id);
        return ApiResponse<SentenceResponse>.SuccessResponse(item);
    }

    /// <summary>
    /// Tạo mới câu ví dụ.
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<SentenceResponse>> Create([FromBody] CreateSentenceRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _sentenceService.CreateAsync(request, userId);
        return ApiResponse<SentenceResponse>.SuccessResponse(item);
    }

    /// <summary>
    /// Cập nhật câu ví dụ theo id.
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ApiResponse<SentenceResponse>> Update([FromRoute] string id, [FromBody] UpdateSentenceRequest request)
    {
        var item = await _sentenceService.UpdateAsync(id, request);
        return ApiResponse<SentenceResponse>.SuccessResponse(item);
    }

    /// <summary>
    /// Xóa câu ví dụ theo id.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] string id)
    {
        var deleted = await _sentenceService.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted);
    }

    private static FileContentResult CreateJsonFileResult<T>(T data, string fileName)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        return new FileContentResult(Encoding.UTF8.GetBytes(json), "application/json")
        {
            FileDownloadName = fileName,
        };
    }
}
