namespace Application.DTOs.Shadowing;

public class ShadowingAttemptResponse
{
    public string AttemptId { get; set; } = string.Empty;
    public string TopicId { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string SentenceId { get; set; } = string.Empty;
    public string SentenceText { get; set; } = string.Empty;
    public string AudioAssetId { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string? RecognizedText { get; set; }
    public double? PronScore { get; set; }
    public double? AccuracyScore { get; set; }
    public double? FluencyScore { get; set; }
    public double? CompletenessScore { get; set; }
    public double? ProsodyScore { get; set; }
    public List<string> ErrorTypes { get; set; } = new();
    public List<ShadowingAttemptWordAssessmentResponse> WordAssessments { get; set; } = new();
    public int? DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShadowingAttemptWordAssessmentResponse
{
    public string Word { get; set; } = string.Empty;
    public string? DisplayWord { get; set; }
    public double? AccuracyScore { get; set; }
    public string? ErrorType { get; set; }
}
