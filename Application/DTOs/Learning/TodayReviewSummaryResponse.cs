namespace Application.DTOs.Learning;

public class TodayReviewSummaryResponse
{
    public string? DeckId { get; set; }
    public List<string> FolderIds { get; set; } = new();
    public int DueCount { get; set; }
    public int TotalCards { get; set; }
}
