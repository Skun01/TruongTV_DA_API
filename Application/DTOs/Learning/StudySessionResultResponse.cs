namespace Application.DTOs.Learning;

public class StudySessionResultResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string? DeckId { get; set; }
    public string? DeckTitle { get; set; }
    public string Mode { get; set; } = string.Empty;
    public int TotalCards { get; set; }
    public int CompletedCards { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public double Accuracy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public StudySessionSettingsResponse Settings { get; set; } = new();
}
