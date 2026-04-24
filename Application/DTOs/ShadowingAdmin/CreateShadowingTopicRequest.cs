namespace Application.DTOs.ShadowingAdmin;

public class CreateShadowingTopicRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
}
