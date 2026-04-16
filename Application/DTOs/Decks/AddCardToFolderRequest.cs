namespace Application.DTOs.Decks;

public class AddCardToFolderRequest
{
    public string CardId { get; set; } = string.Empty;
    public int? Position { get; set; }
}
