using Application.Common;
using Application.DTOs.Exams;
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
[Route("api/exams")]
public class ExamsController : BaseController
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    /// <summary>
    /// Tạo đề thi mới
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<ExamDetailResponse>> CreateExam([FromBody] CreateExamRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.CreateExamAsync(request, userId);
        return ApiResponse<ExamDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tải template import đề thi
    /// </summary>
    [HttpGet("import-template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _examService.GetImportTemplateAsync();
        return CreateJsonFileResult(result, "jlpt-exam-import-template.json");
    }

    /// <summary>
    /// Lấy hướng dẫn import đề thi
    /// </summary>
    [HttpGet("import-guide")]
    public async Task<ApiResponse<ExamImportTemplateGuide>> GetImportGuide()
    {
        var result = await _examService.GetImportGuideAsync();
        return ApiResponse<ExamImportTemplateGuide>.SuccessResponse(result);
    }

    /// <summary>
    /// Xuất dữ liệu đề thi
    /// </summary>
    [HttpGet("{id}/export")]
    public async Task<IActionResult> Export([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.ExportAsync(id, userId);
        return CreateJsonFileResult(result, $"jlpt-exam-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    /// <summary>
    /// Xem trước dữ liệu import đề thi
    /// </summary>
    [HttpPost("preview-import")]
    public async Task<ApiResponse<ExamImportPreviewResponse>> PreviewImport([FromBody] ImportExamRequest request)
    {
        var result = await _examService.PreviewImportAsync(request);
        return ApiResponse<ExamImportPreviewResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xác nhận import đề thi
    /// </summary>
    [HttpPost("commit-import")]
    public async Task<ApiResponse<ExamImportCommitResponse>> CommitImport([FromBody] ImportExamRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.CommitImportAsync(request, userId);
        return ApiResponse<ExamImportCommitResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tìm kiếm danh sách đề thi
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<ExamListItemResponse>>> SearchExams([FromQuery] ExamSearchQuery query)
    {
        var (items, meta) = await _examService.SearchExamsAsync(query);
        return ApiResponse<List<ExamListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết đề thi
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<ExamDetailResponse>> GetExamDetail([FromRoute] string id)
    {
        var result = await _examService.GetExamDetailAsync(id);
        return ApiResponse<ExamDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật đề thi
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse<ExamDetailResponse>> UpdateExam([FromRoute] string id, [FromBody] UpdateExamRequest request)
    {
        var result = await _examService.UpdateExamAsync(id, request);
        return ApiResponse<ExamDetailResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xuất bản đề thi
    /// </summary>
    [HttpPatch("{id}/publish")]
    public async Task<ApiResponse<string>> PublishExam([FromRoute] string id)
    {
        await _examService.PublishExamAsync(id);
        return ApiResponse<string>.SuccessResponse("Published");
    }

    /// <summary>
    /// Xóa đề thi
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> DeleteExam([FromRoute] string id)
    {
        await _examService.DeleteExamAsync(id);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    /// <summary>
    /// Tạo section trong đề thi
    /// </summary>
    // Section endpoints
    [HttpPost("{examId}/sections")]
    public async Task<ApiResponse<ExamSectionResponse>> CreateSection(
        [FromRoute] string examId,
        [FromBody] CreateSectionRequest request)
    {
        var result = await _examService.CreateSectionAsync(examId, request);
        return ApiResponse<ExamSectionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật section trong đề thi
    /// </summary>
    [HttpPut("{examId}/sections/{sectionId}")]
    public async Task<ApiResponse<ExamSectionResponse>> UpdateSection(
        [FromRoute] string examId,
        [FromRoute] string sectionId,
        [FromBody] UpdateSectionRequest request)
    {
        var result = await _examService.UpdateSectionAsync(examId, sectionId, request);
        return ApiResponse<ExamSectionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa section trong đề thi
    /// </summary>
    [HttpDelete("{examId}/sections/{sectionId}")]
    public async Task<ApiResponse<string>> DeleteSection(
        [FromRoute] string examId,
        [FromRoute] string sectionId)
    {
        await _examService.DeleteSectionAsync(examId, sectionId);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    /// <summary>
    /// Tạo nhóm câu hỏi trong section
    /// </summary>
    // QuestionGroup endpoints
    [HttpPost("sections/{sectionId}/groups")]
    public async Task<ApiResponse<QuestionGroupResponse>> CreateQuestionGroup(
        [FromRoute] string sectionId,
        [FromBody] CreateQuestionGroupRequest request)
    {
        var result = await _examService.CreateQuestionGroupAsync(sectionId, request);
        return ApiResponse<QuestionGroupResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật nhóm câu hỏi trong section
    /// </summary>
    [HttpPut("sections/{sectionId}/groups/{groupId}")]
    public async Task<ApiResponse<QuestionGroupResponse>> UpdateQuestionGroup(
        [FromRoute] string sectionId,
        [FromRoute] string groupId,
        [FromBody] UpdateQuestionGroupRequest request)
    {
        var result = await _examService.UpdateQuestionGroupAsync(sectionId, groupId, request);
        return ApiResponse<QuestionGroupResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa nhóm câu hỏi trong section
    /// </summary>
    [HttpDelete("sections/{sectionId}/groups/{groupId}")]
    public async Task<ApiResponse<string>> DeleteQuestionGroup(
        [FromRoute] string sectionId,
        [FromRoute] string groupId)
    {
        await _examService.DeleteQuestionGroupAsync(sectionId, groupId);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    /// <summary>
    /// Sinh audio TTS cho Choukai group từ AudioScript
    /// </summary>
    [HttpPost("groups/{groupId}/generate-audio")]
    public async Task<ApiResponse<QuestionGroupResponse>> GenerateGroupAudio([FromRoute] string groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.GenerateGroupAudioAsync(groupId, userId);
        return ApiResponse<QuestionGroupResponse>.SuccessResponse(result);
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
