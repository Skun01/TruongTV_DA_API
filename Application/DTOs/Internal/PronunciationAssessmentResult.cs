namespace Application.DTOs.Internal;

public class PronunciationAssessmentResult
{
    public string? RecognizedText { get; set; }
    public double? PronScore { get; set; }
    public double? AccuracyScore { get; set; }
    public double? FluencyScore { get; set; }
    public double? CompletenessScore { get; set; }
    public double? ProsodyScore { get; set; }
    public List<string> ErrorTypes { get; set; } = new();
    public List<PronunciationAssessmentWordResult> Words { get; set; } = new();
    public int? DurationMs { get; set; }
    public string? RawJson { get; set; }
}
