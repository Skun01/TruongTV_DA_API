using Application.Common;
using Application.DTOs.Questions;
using Application.IServices;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
[Route("api/questions")]
public class QuestionsController : BaseController
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    /// <summary>
    /// Tạo câu hỏi mới
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<QuestionResponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        var result = await _questionService.CreateQuestionAsync(request);
        return ApiResponse<QuestionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Tìm kiếm danh sách câu hỏi
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<QuestionResponse>>> SearchQuestions([FromQuery] QuestionSearchQuery query)
    {
        var (items, meta) = await _questionService.SearchQuestionsAsync(query);
        return ApiResponse<List<QuestionResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết câu hỏi
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<QuestionResponse>> GetQuestionDetail([FromRoute] string id)
    {
        var result = await _questionService.GetQuestionDetailAsync(id);
        return ApiResponse<QuestionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Cập nhật câu hỏi
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse<QuestionResponse>> UpdateQuestion(
        [FromRoute] string id,
        [FromBody] UpdateQuestionRequest request)
    {
        var result = await _questionService.UpdateQuestionAsync(id, request);
        return ApiResponse<QuestionResponse>.SuccessResponse(result);
    }

    /// <summary>
    /// Xóa câu hỏi
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> DeleteQuestion([FromRoute] string id)
    {
        await _questionService.DeleteQuestionAsync(id);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    /// <summary>
    /// Tạo hàng loạt câu hỏi trong nhóm
    /// </summary>
    [HttpPost("groups/{groupId}/bulk")]
    public async Task<ApiResponse<List<QuestionResponse>>> BulkCreateQuestions(
        [FromRoute] string groupId,
        [FromBody] BulkCreateQuestionsRequest request)
    {
        var result = await _questionService.BulkCreateQuestionsAsync(groupId, request);
        return ApiResponse<List<QuestionResponse>>.SuccessResponse(result);
    }

    /// <summary>
    /// Sắp xếp lại thứ tự câu hỏi trong nhóm
    /// </summary>
    [HttpPut("groups/{groupId}/reorder")]
    public async Task<ApiResponse<string>> ReorderQuestions(
        [FromRoute] string groupId,
        [FromBody] ReorderQuestionsRequest request)
    {
        await _questionService.ReorderQuestionsAsync(groupId, request);
        return ApiResponse<string>.SuccessResponse("Reordered");
    }
}
