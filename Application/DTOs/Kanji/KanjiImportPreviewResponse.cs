namespace Application.DTOs.Kanji;

public class KanjiImportPreviewResponse
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int InvalidItems { get; set; }
    public List<KanjiImportPreviewItemResponse> Items { get; set; } = new();
}
