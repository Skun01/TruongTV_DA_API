using Application.DTOs.Common;

namespace Application.DTOs.Vocabulary;

public class VocabularySearchQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
    public bool CreatedByMe { get; set; } = false;
}
