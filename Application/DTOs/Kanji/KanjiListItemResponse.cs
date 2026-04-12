namespace Application.DTOs.Kanji;

public class KanjiListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string Kanji { get; set; } = string.Empty;
    public int StrokeCount { get; set; }
    public string? HanViet { get; set; }
    public string MeaningVi { get; set; } = string.Empty;
    public int RadicalCount { get; set; }
}
