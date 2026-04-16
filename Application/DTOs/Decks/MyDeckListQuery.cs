using Application.DTOs.Common;

namespace Application.DTOs.Decks;

public class MyDeckListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? TypeId { get; set; }
}
