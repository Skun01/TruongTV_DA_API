namespace Application.DTOs.Learning;

public class TodayReviewQuery
{
    public string? DeckId { get; set; }
    public List<string> FolderIds { get; set; } = new();
}
