namespace Domain.Entities;

public class KanjiDetail
{
    public string CardId { get; set; } = string.Empty;
    public Card Card { get; set; } = null!;

    public string Kanji { get; set; } = string.Empty;
    public int StrokeCount { get; set; }
    public string? StrokeOrderUrl { get; set; }
    public List<string> Onyomi { get; set; } = new();
    public List<string> Kunyomi { get; set; } = new();
    public string? HanViet { get; set; }
    public string MeaningVi { get; set; } = string.Empty;
}
