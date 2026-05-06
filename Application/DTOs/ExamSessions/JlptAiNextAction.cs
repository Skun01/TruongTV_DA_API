namespace Application.DTOs.ExamSessions;

public class JlptAiNextAction
{
    public string Label { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? TargetRoute { get; set; }
}
