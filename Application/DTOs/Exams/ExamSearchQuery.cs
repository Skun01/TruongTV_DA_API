using Application.DTOs.Common;

namespace Application.DTOs.Exams;

public class ExamSearchQuery : PagingQuery
{
    public string? Keyword { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
}
