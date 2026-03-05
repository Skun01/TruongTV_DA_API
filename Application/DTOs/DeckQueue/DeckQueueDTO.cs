using Domain.Enums;

namespace Application.DTOs.DeckQueue;

public class DeckQueueDTO
{
    public string DeckId { set; get; } = string.Empty;
    public string DeckName { set; get; } = string.Empty;
    public DeckType DeckType { set; get; }
    public int TotalCards { set; get; }
    public int LearnedCards { set; get; }
    public int DueForReview { set; get; }
    public DateTime AddedAt { set; get; }
}
