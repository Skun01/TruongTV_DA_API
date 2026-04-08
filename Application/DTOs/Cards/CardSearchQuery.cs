using Application.DTOs.Common;

namespace Application.DTOs.Cards;

public class CardSearchQuery : PagingQuery
{
    public string? CardType { get; set; }
    public string? Q { get; set; }
    public string? Level { get; set; }
}
