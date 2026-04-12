namespace Application.DTOs.Kanji;

public class KanjiImportCommitItemResponse
{
    public int RowNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Kanji { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? CardId { get; set; }
    public List<string> Errors { get; set; } = new();
}
