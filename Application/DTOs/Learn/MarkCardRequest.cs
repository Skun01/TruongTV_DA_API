using Domain.Enums;

namespace Application.DTOs.Learn;

public class MarkCardRequest
{
    public string CardId { set; get; } = string.Empty;
    public DeckType CardType { set; get; }
    public bool IsMastered { set; get; } = false;
}
