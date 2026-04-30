using Application.DTOs.Common;

namespace Application.DTOs.ExamSessions;

public class SessionHistoryQuery : PagingQuery
{
    public string? ExamId { get; set; }
    public string? Status { get; set; }
}
