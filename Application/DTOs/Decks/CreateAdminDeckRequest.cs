namespace Application.DTOs.Decks;

public class CreateAdminDeckRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
    public bool IsOfficial { get; set; }
    public string? TypeId { get; set; }
    public string? CreatedBy { get; set; }
}
