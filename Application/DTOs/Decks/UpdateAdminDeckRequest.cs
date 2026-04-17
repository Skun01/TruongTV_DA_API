namespace Application.DTOs.Decks;

public class UpdateAdminDeckRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
    public bool? IsOfficial { get; set; }
    public string? TypeId { get; set; }
}
