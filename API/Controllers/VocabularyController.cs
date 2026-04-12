using Application.Common;
using Application.DTOs.Vocabulary;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;

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

    /// <summary>
    /// Tìm kiếm danh sách vocabulary cho Editor/Admin.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet]
    public async Task<ApiResponse<List<VocabularyListItemResponse>>> Search([FromQuery] VocabularySearchQuery query)
    {
        var userId = GetCurrentUserId();

        var (items, meta) = await _vocabularyDetailService.SearchAsync(query, userId);

        return ApiResponse<List<VocabularyListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết một vocabulary card theo id.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{cardId}")]
    public async Task<ApiResponse<VocabularyDetailResponse>> GetDetail([FromRoute] string cardId)
    {
        var userId = GetCurrentUserIdOrNull();
        var result = await _vocabularyDetailService.GetDetailAsync(cardId, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tải JSON template mẫu cho import vocabulary.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet("import-template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _vocabularyDetailService.GetImportTemplateAsync();
        return CreateJsonFileResult(result, "vocabulary-import-template.json");
    }

    /// <summary>
    /// Tải JSON export vocabulary theo bộ lọc.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] VocabularyExportQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.ExportAsync(query, userId);
        return CreateJsonFileResult(result, $"vocabulary-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    /// <summary>
    /// Preview payload import vocabulary, validate theo từng item, chưa ghi DB.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("import/preview")]
    public async Task<ApiResponse<VocabularyImportPreviewResponse>> PreviewImport([FromBody] ImportVocabularyRequest request)
    {
        var result = await _vocabularyDetailService.PreviewImportAsync(request);
        return ApiResponse<VocabularyImportPreviewResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Commit batch import vocabulary vào DB.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("import/commit")]
    public async Task<ApiResponse<VocabularyImportCommitResponse>> CommitImport([FromBody] ImportVocabularyRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.CommitImportAsync(request, userId);
        return ApiResponse<VocabularyImportCommitResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tạo mới một vocabulary card.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost]
    public async Task<ApiResponse<VocabularyDetailResponse>> Create([FromBody] CreateVocabularyCardRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.CreateAsync(request, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật thông tin một vocabulary card.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPatch("{cardId}")]
    public async Task<ApiResponse<VocabularyDetailResponse>> Update([FromRoute] string cardId, [FromBody] UpdateVocabularyCardRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.UpdateAsync(cardId, request, userId);
        return ApiResponse<VocabularyDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa mềm một vocabulary card (chuyển trạng thái Archived).
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpDelete("{cardId}")]
    public async Task<ApiResponse<bool>> SoftDelete([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyDetailService.SoftDeleteAsync(cardId, userId);
        return ApiResponse<bool>.SuccessResponse(result);
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
