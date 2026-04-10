namespace Application.DTOs.Cards;

public class CardListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> AlternateForms { get; set; } = new();
}
