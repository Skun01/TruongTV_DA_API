namespace Application.DTOs.Shadowing;

public class ShadowingAttemptHistoryItemResponse
{
    public string AttemptId { get; set; } = string.Empty;
    public string TopicId { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string SentenceId { get; set; } = string.Empty;
    public string SentenceText { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public double? PronScore { get; set; }
    public double? AccuracyScore { get; set; }
    public double? FluencyScore { get; set; }
    public double? CompletenessScore { get; set; }
    public double? ProsodyScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
