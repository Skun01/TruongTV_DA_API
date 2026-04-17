using Application.DTOs.Common;

namespace Application.DTOs.Decks;

public class AdminDeckTypeListQuery : PagingQuery
{
    public string? Q { get; set; }
}
