using Domain.Enums;

namespace Domain.Entities;

public class ExamSessionAiAnalysis : BaseEntity
{
    public string ExamSessionId { get; set; } = string.Empty;
    public ExamSession ExamSession { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string PromptVersion { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public ExamSessionAiAnalysisStatus Status { get; set; } = ExamSessionAiAnalysisStatus.Completed;
    public string InputHash { get; set; } = string.Empty;
    public string OutputJson { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int? LatencyMs { get; set; }
    public ExamSessionAiAnalysisTriggerType TriggerType { get; set; } = ExamSessionAiAnalysisTriggerType.AutoGenerate;
    public string? TriggerReason { get; set; }
}
