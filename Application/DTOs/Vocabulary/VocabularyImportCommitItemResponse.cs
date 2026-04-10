namespace Application.DTOs.Vocabulary;

public class VocabularyImportCommitItemResponse
{
    public int RowNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Writing { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Action { get; set; } = "skipped";
    public string? CardId { get; set; }
    public List<string> Errors { get; set; } = new();
}
