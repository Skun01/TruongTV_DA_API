namespace Domain.Entities;

public class GrammarResource
{
    public string Id { get; set; } = string.Empty;

    public string CardId { get; set; } = string.Empty;
    public Card Card { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
