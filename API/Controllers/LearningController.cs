using Application.Common;
using Application.DTOs.Learning;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/learning")]
public class LearningController : BaseController
{
    private readonly ILearningService _learningService;

    public LearningController(ILearningService learningService)
    {
        _learningService = learningService;
    }

    [HttpPost("sessions")]
    public async Task<ApiResponse<StudySessionResponse>> CreateSession([FromBody] CreateStudySessionRequest request)
    {
        var result = await _learningService.CreateSessionAsync(request, GetCurrentUserId());
        return ApiResponse<StudySessionResponse>.SuccessResponse(result);
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<ApiResponse<StudySessionResponse>> GetSession([FromRoute] string sessionId)
    {
        var result = await _learningService.GetSessionAsync(sessionId, GetCurrentUserId());
        return ApiResponse<StudySessionResponse>.SuccessResponse(result);
    }

    [HttpGet("sessions/{sessionId}/next")]
    public async Task<ApiResponse<StudyQuestionResponse?>> GetNextQuestion([FromRoute] string sessionId)
    {
        var result = await _learningService.GetNextQuestionAsync(sessionId, GetCurrentUserId());
        return ApiResponse<StudyQuestionResponse?>.SuccessResponse(result);
    }

    [HttpPost("sessions/{sessionId}/submit")]
    public async Task<ApiResponse<SubmitStudyAnswerResponse>> SubmitAnswer([FromRoute] string sessionId, [FromBody] SubmitStudyAnswerRequest request)
    {
        var result = await _learningService.SubmitAnswerAsync(sessionId, request, GetCurrentUserId());
        return ApiResponse<SubmitStudyAnswerResponse>.SuccessResponse(result);
    }

    [HttpGet("review/today")]
    public async Task<ApiResponse<TodayReviewSummaryResponse>> GetTodayReview([FromQuery] TodayReviewQuery query)
    {
        var result = await _learningService.GetTodayReviewAsync(query, GetCurrentUserId());
        return ApiResponse<TodayReviewSummaryResponse>.SuccessResponse(result);
    }
}
