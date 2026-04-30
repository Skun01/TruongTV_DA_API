using Application.Common;
using Application.DTOs.Exams;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<ApiResponse<ExamDetailResponse>> CreateExam([FromBody] CreateExamRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.CreateExamAsync(request, userId);
        return ApiResponse<ExamDetailResponse>.SuccessResponse(result);
    }

    [HttpGet]
    public async Task<ApiResponse<List<ExamListItemResponse>>> SearchExams([FromQuery] ExamSearchQuery query)
    {
        var (items, meta) = await _examService.SearchExamsAsync(query);
        return ApiResponse<List<ExamListItemResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<ExamDetailResponse>> GetExamDetail([FromRoute] string id)
    {
        var result = await _examService.GetExamDetailAsync(id);
        return ApiResponse<ExamDetailResponse>.SuccessResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<ExamDetailResponse>> UpdateExam([FromRoute] string id, [FromBody] UpdateExamRequest request)
    {
        var result = await _examService.UpdateExamAsync(id, request);
        return ApiResponse<ExamDetailResponse>.SuccessResponse(result);
    }

    [HttpPatch("{id}/publish")]
    public async Task<ApiResponse<string>> PublishExam([FromRoute] string id)
    {
        await _examService.PublishExamAsync(id);
        return ApiResponse<string>.SuccessResponse("Published");
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> DeleteExam([FromRoute] string id)
    {
        await _examService.DeleteExamAsync(id);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    // Section endpoints
    [HttpPost("{examId}/sections")]
    public async Task<ApiResponse<ExamSectionResponse>> CreateSection(
        [FromRoute] string examId,
        [FromBody] CreateSectionRequest request)
    {
        var result = await _examService.CreateSectionAsync(examId, request);
        return ApiResponse<ExamSectionResponse>.SuccessResponse(result);
    }

    [HttpPut("{examId}/sections/{sectionId}")]
    public async Task<ApiResponse<ExamSectionResponse>> UpdateSection(
        [FromRoute] string examId,
        [FromRoute] string sectionId,
        [FromBody] UpdateSectionRequest request)
    {
        var result = await _examService.UpdateSectionAsync(examId, sectionId, request);
        return ApiResponse<ExamSectionResponse>.SuccessResponse(result);
    }

    [HttpDelete("{examId}/sections/{sectionId}")]
    public async Task<ApiResponse<string>> DeleteSection(
        [FromRoute] string examId,
        [FromRoute] string sectionId)
    {
        await _examService.DeleteSectionAsync(examId, sectionId);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    // QuestionGroup endpoints
    [HttpPost("sections/{sectionId}/groups")]
    public async Task<ApiResponse<QuestionGroupResponse>> CreateQuestionGroup(
        [FromRoute] string sectionId,
        [FromBody] CreateQuestionGroupRequest request)
    {
        var result = await _examService.CreateQuestionGroupAsync(sectionId, request);
        return ApiResponse<QuestionGroupResponse>.SuccessResponse(result);
    }

    [HttpPut("sections/{sectionId}/groups/{groupId}")]
    public async Task<ApiResponse<QuestionGroupResponse>> UpdateQuestionGroup(
        [FromRoute] string sectionId,
        [FromRoute] string groupId,
        [FromBody] UpdateQuestionGroupRequest request)
    {
        var result = await _examService.UpdateQuestionGroupAsync(sectionId, groupId, request);
        return ApiResponse<QuestionGroupResponse>.SuccessResponse(result);
    }

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
}
