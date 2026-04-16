using Application.DTOs.Common;

namespace Application.DTOs.Decks;

public class BookmarkedDeckListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? TypeId { get; set; }
}
