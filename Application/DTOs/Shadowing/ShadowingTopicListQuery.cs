using Application.DTOs.Common;

namespace Application.DTOs.Shadowing;

public class ShadowingTopicListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public string? Visibility { get; set; }
    public bool? OfficialOnly { get; set; }
}
