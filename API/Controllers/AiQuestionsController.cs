using Application.Common;
using Application.DTOs.AiQuestions;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
[Route("api/ai/questions")]
public class AiQuestionsController : BaseController
{
    private readonly IAiQuestionService _aiQuestionService;

    public AiQuestionsController(IAiQuestionService aiQuestionService)
    {
        _aiQuestionService = aiQuestionService;
    }

    /// <summary>
    /// Sinh câu hỏi tự động bằng AI
    /// </summary>
    [HttpPost("generate")]
    public async Task<ApiResponse<List<AiGeneratedQuestionResponse>>> GenerateQuestions(
        [FromBody] GenerateQuestionsRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _aiQuestionService.GenerateQuestionsAsync(request, userId);
        return ApiResponse<List<AiGeneratedQuestionResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Danh sách câu hỏi AI đã sinh
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<AiGeneratedQuestionResponse>>> SearchAiQuestions(
        [FromQuery] AiQuestionSearchQuery query)
    {
        var (items, meta) = await _aiQuestionService.SearchAsync(query);
        return ApiResponse<List<AiGeneratedQuestionResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Chi tiết câu hỏi AI
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<AiGeneratedQuestionResponse>> GetAiQuestionDetail([FromRoute] string id)
    {
        var result = await _aiQuestionService.GetDetailAsync(id);
        return ApiResponse<AiGeneratedQuestionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Duyệt câu hỏi AI → lưu vào Question Bank
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ApiResponse<AiGeneratedQuestionResponse>> ApproveAiQuestion([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _aiQuestionService.ApproveAsync(id, userId);
        return ApiResponse<AiGeneratedQuestionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Từ chối câu hỏi AI
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ApiResponse<AiGeneratedQuestionResponse>> RejectAiQuestion([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await _aiQuestionService.RejectAsync(id, userId);
        return ApiResponse<AiGeneratedQuestionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Chỉnh sửa câu hỏi AI trước khi duyệt
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse<AiGeneratedQuestionResponse>> EditAiQuestion(
        [FromRoute] string id,
        [FromBody] EditAiQuestionRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _aiQuestionService.EditAsync(id, request, userId);
        return ApiResponse<AiGeneratedQuestionResponse>.SuccessResponse(result);
    }
}
