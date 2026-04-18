namespace Application.DTOs.Learning;

public class DueReviewCardsResponse
{
    public int DueCount { get; set; }
    public List<string> CardIds { get; set; } = new();
}
