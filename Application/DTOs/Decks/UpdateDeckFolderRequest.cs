namespace Application.DTOs.Decks;

public class UpdateDeckFolderRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
