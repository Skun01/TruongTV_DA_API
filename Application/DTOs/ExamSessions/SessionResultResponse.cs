namespace Application.DTOs.ExamSessions;

public class SessionResultResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public bool IsPassed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<SectionScoreResponse> SectionScores { get; set; } = new();
    public List<ResultQuestionResponse> Questions { get; set; } = new();
}
