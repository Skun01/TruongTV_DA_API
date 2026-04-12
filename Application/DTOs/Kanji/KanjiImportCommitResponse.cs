namespace Application.DTOs.Kanji;

public class KanjiImportCommitResponse
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public bool HasValidationErrors { get; set; }
    public List<KanjiImportCommitItemResponse> Items { get; set; } = new();
}
