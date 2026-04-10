namespace Application.DTOs.Sentences;

public class SentenceImportCommitItemResponse
{
    public int RowNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Action { get; set; } = "skipped";
    public string? SentenceId { get; set; }
    public List<string> Errors { get; set; } = new();
}
