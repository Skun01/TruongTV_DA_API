namespace Application.DTOs.ExamSessions;

public class JlptAiRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public string? TargetRoute { get; set; }
    public List<string> TargetIds { get; set; } = new();
}
