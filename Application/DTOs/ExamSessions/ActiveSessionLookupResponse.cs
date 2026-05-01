namespace Application.DTOs.ExamSessions;

public class ActiveSessionLookupResponse
{
    public bool HasActiveSession { get; set; }
    public string? SessionId { get; set; }
}
