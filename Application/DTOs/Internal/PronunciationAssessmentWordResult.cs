namespace Application.DTOs.Internal;

public class PronunciationAssessmentWordResult
{
    public string Word { get; set; } = string.Empty;
    public string? DisplayWord { get; set; }
    public double? AccuracyScore { get; set; }
    public string? ErrorType { get; set; }
}
