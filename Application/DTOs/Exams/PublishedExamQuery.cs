using Application.DTOs.Common;

namespace Application.DTOs.Exams;

public class PublishedExamQuery : PagingQuery
{
    public string? Keyword { get; set; }
    public string? Level { get; set; }
}
