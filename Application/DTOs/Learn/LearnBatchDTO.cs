namespace Application.DTOs.Learn;

public class LearnBatchDTO
{
    public string DeckId { set; get; } = string.Empty;
    public string DeckName { set; get; } = string.Empty;
    public List<LearnCardDTO> Cards { set; get; } = [];
    public int TotalNewCards { set; get; }
    public DailyProgressDTO DailyProgress { set; get; } = null!;
}
