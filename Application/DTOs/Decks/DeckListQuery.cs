using Application.DTOs.Common;

namespace Application.DTOs.Decks;

public class DeckListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? TypeId { get; set; }
    public bool? OfficialOnly { get; set; }
}
