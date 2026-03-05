namespace Application.DTOs.Review;

public class ReviewSessionDTO
{
    public string DeckId { set; get; } = string.Empty;
    public string DeckName { set; get; } = string.Empty;
    public int TotalDue { set; get; }
    public List<ReviewItemDTO> Items { set; get; } = [];
}
