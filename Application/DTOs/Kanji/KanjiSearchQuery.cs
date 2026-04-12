using Application.DTOs.Common;

namespace Application.DTOs.Kanji;

public class KanjiSearchQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
    public int? StrokeCountMin { get; set; }
    public int? StrokeCountMax { get; set; }
    public string? Radical { get; set; }
    public bool CreatedByMe { get; set; } = false;
}
