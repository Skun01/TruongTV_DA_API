namespace Application.DTOs.ExamSessions;

public class JlptAiAnalysisSummary
{
    public string Headline { get; set; } = string.Empty;
    public string OverallBand { get; set; } = string.Empty;
    public double ScorePercent { get; set; }
    public bool Passed { get; set; }
    public string EstimatedLevelReadiness { get; set; } = string.Empty;
}
