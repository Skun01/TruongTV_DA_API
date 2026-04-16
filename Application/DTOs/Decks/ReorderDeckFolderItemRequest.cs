namespace Application.DTOs.Decks;

public class ReorderDeckFolderItemRequest
{
    public string FolderId { get; set; } = string.Empty;
    public int Position { get; set; }
}
