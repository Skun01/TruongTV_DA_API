using Application.DTOs.Common;

namespace Application.DTOs.Questions;

public class QuestionSearchQuery : PagingQuery
{
    public string? Keyword { get; set; }
    public string? Level { get; set; }
    public string? SectionType { get; set; }
}
