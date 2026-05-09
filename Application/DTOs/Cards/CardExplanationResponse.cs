namespace Application.DTOs.Cards;

public class CardExplanationResponse
{
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
