using Application.DTOs.Common;

namespace Application.DTOs.LearningAdmin;

public class LearningAdminCardIssuesQuery : PagingQuery
{
    public string? CardType { get; set; }
    public string? Mode { get; set; }
    public string? IssueType { get; set; }
    public string? Q { get; set; }
    public string? DeckId { get; set; }
}
