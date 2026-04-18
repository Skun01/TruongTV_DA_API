namespace Application.DTOs.Learning;

public class StudySessionResponse
{
    public string Id { get; set; } = string.Empty;
    public string DeckId { get; set; } = string.Empty;
    public string DeckTitle { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public List<string> FolderIds { get; set; } = new();
    public int TotalCards { get; set; }
    public int CompletedCards { get; set; }
    public int RemainingCards { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
