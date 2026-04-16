namespace Application.DTOs.Decks;

public class CreateDeckFolderRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Position { get; set; }
}
