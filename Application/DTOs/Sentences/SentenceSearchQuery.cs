using Application.DTOs.Common;

namespace Application.DTOs.Sentences;

public class SentenceSearchQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public bool CreatedByMe { get; set; } = false;
    public bool? HasAudio { get; set; }
}
