using Application.DTOs.Common;

namespace Application.DTOs.ShadowingAdmin;

public class AdminShadowingTopicListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
    public bool? IsOfficial { get; set; }
    public string? CreatedBy { get; set; }
}
