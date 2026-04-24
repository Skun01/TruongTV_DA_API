using Application.DTOs.Common;

namespace Application.DTOs.Shadowing;

public class ShadowingAttemptHistoryQuery : PagingQuery
{
    public string? TopicId { get; set; }
    public string? SentenceId { get; set; }
}
