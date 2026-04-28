namespace Application.DTOs.ShadowingAdmin;

public class UpdateShadowingTopicRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Level { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
}
