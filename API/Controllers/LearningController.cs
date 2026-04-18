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

    /// <summary>
    /// Tạo một phiên học mới cho user theo deck hoặc theo danh sách card đã chọn.
    /// Nếu không truyền settings, backend sẽ lấy theo user default settings.
    /// </summary>
    [HttpPost("sessions")]
    public async Task<ApiResponse<StudySessionResponse>> CreateSession([FromBody] CreateStudySessionRequest request)
    {
        var result = await _learningService.CreateSessionAsync(request, GetCurrentUserId());
        return ApiResponse<StudySessionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy thông tin tổng quan của một phiên học hiện có.
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<ApiResponse<StudySessionResponse>> GetSession([FromRoute] string sessionId)
    {
        var result = await _learningService.GetSessionAsync(sessionId, GetCurrentUserId());
        return ApiResponse<StudySessionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa một phiên học của user hiện tại.
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<ApiResponse<bool>> DeleteSession([FromRoute] string sessionId)
    {
        var result = await _learningService.DeleteSessionAsync(sessionId, GetCurrentUserId());
        return ApiResponse<bool>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy lịch sử các phiên học gần đây của user hiện tại.
    /// </summary>
    [HttpGet("history")]
    public async Task<ApiResponse<List<StudySessionResponse>>> GetHistory([FromQuery] StudyHistoryQuery query)
    {
        var result = await _learningService.GetHistoryAsync(query, GetCurrentUserId());
        return ApiResponse<List<StudySessionResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy câu hỏi tiếp theo trong phiên học hiện tại.
    /// Trả về null khi phiên học đã hoàn thành hoặc không còn card để học.
    /// </summary>
    [HttpGet("sessions/{sessionId}/next")]
    public async Task<ApiResponse<StudyQuestionResponse?>> GetNextQuestion([FromRoute] string sessionId)
    {
        var result = await _learningService.GetNextQuestionAsync(sessionId, GetCurrentUserId());
        return ApiResponse<StudyQuestionResponse?>.SuccessResponse(result);
    }

    /// <summary>
    /// Nộp câu trả lời cho card hiện tại trong phiên học.
    /// Fill-in dùng answers, multiple-choice dùng selectedOptionIds, flashcard dùng flashcardResult.
    /// </summary>
    [HttpPost("sessions/{sessionId}/submit")]
    public async Task<ApiResponse<SubmitStudyAnswerResponse>> SubmitAnswer([FromRoute] string sessionId, [FromBody] SubmitStudyAnswerRequest request)
    {
        var result = await _learningService.SubmitAnswerAsync(sessionId, request, GetCurrentUserId());
        return ApiResponse<SubmitStudyAnswerResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy kết quả tổng kết của một phiên học.
    /// </summary>
    [HttpGet("sessions/{sessionId}/result")]
    public async Task<ApiResponse<StudySessionResultResponse>> GetSessionResult([FromRoute] string sessionId)
    {
        var result = await _learningService.GetSessionResultAsync(sessionId, GetCurrentUserId());
        return ApiResponse<StudySessionResultResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tạo lại một phiên học mới từ scope và settings của phiên cũ.
    /// </summary>
    [HttpPost("sessions/{sessionId}/restart")]
    public async Task<ApiResponse<StudySessionResponse>> RestartSession([FromRoute] string sessionId)
    {
        var result = await _learningService.RestartSessionAsync(sessionId, GetCurrentUserId());
        return ApiResponse<StudySessionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy thống kê số card đến hạn ôn của user trong hôm nay.
    /// Có thể lọc theo deck hoặc tập folder cụ thể.
    /// </summary>
    [HttpGet("review/today")]
    public async Task<ApiResponse<TodayReviewSummaryResponse>> GetTodayReview([FromQuery] TodayReviewQuery query)
    {
        var result = await _learningService.GetTodayReviewAsync(query, GetCurrentUserId());
        return ApiResponse<TodayReviewSummaryResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy danh sách card progress đã đến hạn ôn toàn cục của user hiện tại.
    /// Có thể giới hạn số lượng card trả về.
    /// </summary>
    [HttpGet("review/due-cards")]
    public async Task<ApiResponse<DueReviewCardsResponse>> GetDueCards([FromQuery] DueReviewCardsQuery query)
    {
        var result = await _learningService.GetDueCardsAsync(query, GetCurrentUserId());
        return ApiResponse<DueReviewCardsResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Lấy tiến độ học của user trên một card cụ thể.
    /// </summary>
    [HttpGet("progress/cards/{cardId}")]
    public async Task<ApiResponse<CardProgressResponse>> GetCardProgress([FromRoute] string cardId)
    {
        var result = await _learningService.GetCardProgressAsync(cardId, GetCurrentUserId());
        return ApiResponse<CardProgressResponse>.SuccessResponse(result);
    }
}
