namespace Application.DTOs.Learning;

public class StudySessionResponse
{
    public string Id { get; set; } = string.Empty;
    public string? DeckId { get; set; }
    public string? DeckTitle { get; set; }
    public string Mode { get; set; } = string.Empty;
    public List<string> FolderIds { get; set; } = new();
    public int TotalCards { get; set; }
    public int CompletedCards { get; set; }
    public int RemainingCards { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int SubmittedAttempts { get; set; }
    public int RetryCards { get; set; }
    public List<string> SkippedCardIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public StudySessionSettingsResponse Settings { get; set; } = new();
}
