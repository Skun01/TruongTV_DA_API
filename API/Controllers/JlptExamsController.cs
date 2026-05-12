using Application.Common;
using Application.DTOs.Exams;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/jlpt-exams")]
public class JlptExamsController : BaseController
{
    private readonly IExamService _examService;

    public JlptExamsController(IExamService examService)
    {
        _examService = examService;
    }

    /// <summary>
    /// Tìm kiếm danh sách đề thi JLPT đã xuất bản
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponse<List<PublishedExamListItemResponse>>> Search([FromQuery] PublishedExamQuery query)
    {
        var (items, meta) = await _examService.SearchPublishedExamsAsync(query);
        return ApiResponse<List<PublishedExamListItemResponse>>.SuccessResponse(items, meta);
    }

    /// <summary>
    /// Lấy chi tiết đề thi JLPT đã xuất bản
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<PublishedExamDetailResponse>> GetDetail([FromRoute] string id)
    {
        var result = await _examService.GetPublishedExamDetailAsync(id);
        return ApiResponse<PublishedExamDetailResponse>.SuccessResponse(result);
    }
}
