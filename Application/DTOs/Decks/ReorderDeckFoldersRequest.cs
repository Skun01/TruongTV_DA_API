namespace Application.DTOs.Decks;

public class ReorderDeckFoldersRequest
{
    public List<ReorderDeckFolderItemRequest> Items { get; set; } = new();
}
