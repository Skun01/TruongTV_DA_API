using Application.Common;
using Application.DTOs.Grammar;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

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

    /// <summary>
    /// Tìm kiếm danh sách grammar cho Editor/Admin có phân trang.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet]
    public async Task<ApiResponse<List<GrammarListItemResponse>>> Search([FromQuery] GrammarSearchQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _grammarService.SearchAsync(query, userId);
        return ApiResponse<List<GrammarListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết grammar card theo cardId. Card Published: public, chưa Published: chỉ owner mới xem được.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{cardId}")]
    public async Task<ApiResponse<GrammarDetailResponse>> GetDetail([FromRoute] string cardId)
    {
        var userId = GetCurrentUserIdOrNull();
        var item = await _grammarService.GetDetailAsync(cardId, userId);
        return ApiResponse<GrammarDetailResponse>.SuccessResponse(item);
    }

    /// <summary>
    /// Tạo mới một grammar card.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost]
    public async Task<ApiResponse<GrammarDetailResponse>> Create([FromBody] CreateGrammarCardRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _grammarService.CreateAsync(request, userId);
        return ApiResponse<GrammarDetailResponse>.SuccessResponse(item);
    }

    /// <summary>
    /// Tải JSON template mẫu cho import grammar.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet("import-template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _grammarService.GetImportTemplateAsync();
        return CreateJsonFileResult(result, "grammar-import-template.json");
    }

    /// <summary>
    /// Tải JSON export grammar theo bộ lọc.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] GrammarExportQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _grammarService.ExportAsync(query, userId);
        return CreateJsonFileResult(result, $"grammar-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    /// <summary>
    /// Preview payload import grammar, validate theo từng item, chưa ghi DB.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("import/preview")]
    public async Task<ApiResponse<GrammarImportPreviewResponse>> PreviewImport([FromBody] ImportGrammarRequest request)
    {
        var result = await _grammarService.PreviewImportAsync(request);
        return ApiResponse<GrammarImportPreviewResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Commit batch import grammar vào DB.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("import/commit")]
    public async Task<ApiResponse<GrammarImportCommitResponse>> CommitImport([FromBody] ImportGrammarRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _grammarService.CommitImportAsync(request, userId);
        return ApiResponse<GrammarImportCommitResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật thông tin grammar card theo cardId.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPatch("{cardId}")]
    public async Task<ApiResponse<GrammarDetailResponse>> Update([FromRoute] string cardId, [FromBody] UpdateGrammarCardRequest request)
    {
        var userId = GetCurrentUserId();
        var item = await _grammarService.UpdateAsync(cardId, request, userId);
        return ApiResponse<GrammarDetailResponse>.SuccessResponse(item);
    }

    /// <summary>
    /// Xóa mềm grammar card (chuyển trạng thái Archived).
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpDelete("{cardId}")]
    public async Task<ApiResponse<bool>> SoftDelete([FromRoute] string cardId)
    {
        var userId = GetCurrentUserId();
        var result = await _grammarService.SoftDeleteAsync(cardId, userId);
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
