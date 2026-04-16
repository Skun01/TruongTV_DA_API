namespace Application.DTOs.Decks;

public class CreateMyDeckRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Visibility { get; set; }
    public string? TypeId { get; set; }
}
