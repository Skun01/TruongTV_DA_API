using Application.DTOs.Common;

namespace Application.DTOs.Decks;

public class AdminDeckListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? TypeId { get; set; }
    public string? CreatedBy { get; set; }
    public string? Status { get; set; }
    public string? Visibility { get; set; }
    public bool? IsOfficial { get; set; }
}
