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

    [HttpPost]
    public async Task<ApiResponse<QuestionResponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        var result = await _questionService.CreateQuestionAsync(request);
        return ApiResponse<QuestionResponse>.SuccessResponse(result);
    }

    [HttpGet]
    public async Task<ApiResponse<List<QuestionResponse>>> SearchQuestions([FromQuery] QuestionSearchQuery query)
    {
        var (items, meta) = await _questionService.SearchQuestionsAsync(query);
        return ApiResponse<List<QuestionResponse>>.SuccessResponse(items, meta);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<QuestionResponse>> GetQuestionDetail([FromRoute] string id)
    {
        var result = await _questionService.GetQuestionDetailAsync(id);
        return ApiResponse<QuestionResponse>.SuccessResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<QuestionResponse>> UpdateQuestion(
        [FromRoute] string id,
        [FromBody] UpdateQuestionRequest request)
    {
        var result = await _questionService.UpdateQuestionAsync(id, request);
        return ApiResponse<QuestionResponse>.SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> DeleteQuestion([FromRoute] string id)
    {
        await _questionService.DeleteQuestionAsync(id);
        return ApiResponse<string>.SuccessResponse("Deleted");
    }

    [HttpPost("groups/{groupId}/bulk")]
    public async Task<ApiResponse<List<QuestionResponse>>> BulkCreateQuestions(
        [FromRoute] string groupId,
        [FromBody] BulkCreateQuestionsRequest request)
    {
        var result = await _questionService.BulkCreateQuestionsAsync(groupId, request);
        return ApiResponse<List<QuestionResponse>>.SuccessResponse(result);
    }

    [HttpPut("groups/{groupId}/reorder")]
    public async Task<ApiResponse<string>> ReorderQuestions(
        [FromRoute] string groupId,
        [FromBody] ReorderQuestionsRequest request)
    {
        await _questionService.ReorderQuestionsAsync(groupId, request);
        return ApiResponse<string>.SuccessResponse("Reordered");
    }
}
