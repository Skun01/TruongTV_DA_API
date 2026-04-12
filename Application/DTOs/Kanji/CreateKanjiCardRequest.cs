namespace Application.DTOs.Kanji;

public class CreateKanjiCardRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Status { get; set; }

    public string Kanji { get; set; } = string.Empty;
    public int StrokeCount { get; set; }
    public string? StrokeOrderUrl { get; set; }
    public List<string> Onyomi { get; set; } = new();
    public List<string> Kunyomi { get; set; } = new();
    public string? HanViet { get; set; }
    public string MeaningVi { get; set; } = string.Empty;
    public List<KanjiRadicalUpsertRequest> Radicals { get; set; } = new();
}
