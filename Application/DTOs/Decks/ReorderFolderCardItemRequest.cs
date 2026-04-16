namespace Application.DTOs.Decks;

public class ReorderFolderCardItemRequest
{
    public string CardId { get; set; } = string.Empty;
    public int Position { get; set; }
}
