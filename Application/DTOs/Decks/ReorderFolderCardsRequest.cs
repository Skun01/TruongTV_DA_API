namespace Application.DTOs.Decks;

public class ReorderFolderCardsRequest
{
    public List<ReorderFolderCardItemRequest> Items { get; set; } = new();
}
