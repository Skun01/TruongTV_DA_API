using Application.DTOs.Common;

namespace Application.DTOs.AiQuestions;

public class AiQuestionSearchQuery : PagingQuery
{
    public string? Level { get; set; }
    public string? SectionType { get; set; }
    public string? Status { get; set; }
}
