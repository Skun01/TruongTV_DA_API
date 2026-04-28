using Application.DTOs.Common;

namespace Application.DTOs.ShadowingAdmin;

public class AdminShadowingAvailableSentenceQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public bool? HasAudio { get; set; }
}
