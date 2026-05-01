using Application.DTOs.Exams;

namespace Application.DTOs.ExamSessions;

public class SessionStartResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime ServerNow { get; set; }
    public List<SessionSectionResponse> Sections { get; set; } = new();
}
