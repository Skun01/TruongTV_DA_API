namespace Domain.Entities;

public class KanjiRadical
{
    public string KanjiId { get; set; } = string.Empty;
    public Card KanjiCard { get; set; } = null!;

    public string RadicalId { get; set; } = string.Empty;
    public RadicalDetail Radical { get; set; } = null!;
}
