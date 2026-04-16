namespace Application.DTOs.Decks;

public class DeckFolderResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Position { get; set; }
    public int CardsCount { get; set; }
    public List<DeckFolderCardItemResponse> Cards { get; set; } = new();
}
