using Application.DTOs.Common;

namespace Application.DTOs.Cards;

public class TopicSuggestQuery : PagingQuery
{
    public string Topic { get; set; } = string.Empty;
    public string? CardType { get; set; }
    public string? Level { get; set; }
}
