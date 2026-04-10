using Application.DTOs.Common;

namespace Application.DTOs.Grammar;

public class GrammarSearchQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
    public string? Register { get; set; }
    public bool CreatedByMe { get; set; } = false;
}
