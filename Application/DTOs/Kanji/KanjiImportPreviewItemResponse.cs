namespace Application.DTOs.Kanji;

public class KanjiImportPreviewItemResponse
{
    public int RowNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Kanji { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
