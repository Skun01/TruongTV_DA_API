using Application.Common;
using Application.DTOs.ExamSessions;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/exam-sessions")]
public class ExamSessionsController : BaseController
{
    private readonly IExamSessionService _examSessionService;
    private readonly IExamSessionAiAnalysisService _examSessionAiAnalysisService;

    public ExamSessionsController(
        IExamSessionService examSessionService,
        IExamSessionAiAnalysisService examSessionAiAnalysisService)
    {
        _examSessionService = examSessionService;
        _examSessionAiAnalysisService = examSessionAiAnalysisService;
    }

    /// <summary>
    /// Bắt đầu làm bài thi — trả về toàn bộ đề (không có đáp án)
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<SessionStartResponse>> StartSession([FromBody] StartSessionRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionService.StartSessionAsync(request, userId);
        return ApiResponse<SessionStartResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy lại trạng thái bài làm (resume khi mất kết nối)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<SessionStartResponse>> GetSession([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionService.GetSessionAsync(id, userId);
        return ApiResponse<SessionStartResponse>.SuccessResponse(result);
    }

    [HttpGet("active")]
    public async Task<ApiResponse<ActiveSessionLookupResponse>> GetActiveSession([FromQuery] ActiveSessionQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionService.GetActiveSessionAsync(query, userId);
        return ApiResponse<ActiveSessionLookupResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Auto-save từng câu trả lời
    /// </summary>
    [HttpPost("{id}/answers")]
    public async Task<ApiResponse<SaveAnswerResponse>> SaveAnswer(
        [FromRoute] string id,
        [FromBody] SaveAnswerRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionService.SaveAnswerAsync(id, request, userId);
        return ApiResponse<SaveAnswerResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Nộp bài — tính điểm và trả về kết quả tổng
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ApiResponse<SubmitSessionResponse>> SubmitSession([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionService.SubmitSessionAsync(id, userId);
        return ApiResponse<SubmitSessionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xem kết quả chi tiết sau khi nộp bài
    /// </summary>
    [HttpGet("{id}/result")]
    public async Task<ApiResponse<SessionResultResponse>> GetSessionResult([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionService.GetSessionResultAsync(id, userId);
        return ApiResponse<SessionResultResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy AI analysis cho kết quả bài thi đã nộp
    /// </summary>
    [HttpGet("{id}/ai-analysis")]
    public async Task<ApiResponse<JlptAiAnalysisResponse>> GetAiAnalysis([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionAiAnalysisService.GetAsync(id, userId);
        return ApiResponse<JlptAiAnalysisResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Generate lại AI analysis theo yêu cầu của học viên
    /// </summary>
    [HttpPost("{id}/ai-analysis/regenerate")]
    public async Task<ApiResponse<JlptAiAnalysisResponse>> RegenerateAiAnalysis(
        [FromRoute] string id,
        [FromBody] RegenerateJlptAiAnalysisRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _examSessionAiAnalysisService.RegenerateAsync(id, request, userId);
        return ApiResponse<JlptAiAnalysisResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lịch sử bài thi của học viên
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<SessionListItemResponse>>> GetSessionHistory([FromQuery] SessionHistoryQuery query)
    {
        var userId = GetCurrentUserId();
        var (items, meta) = await _examSessionService.GetSessionHistoryAsync(query, userId);
        return ApiResponse<List<SessionListItemResponse>>.SuccessResponse(items, meta);
    }
}
