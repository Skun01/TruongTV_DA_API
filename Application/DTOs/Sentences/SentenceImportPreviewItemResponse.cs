namespace Application.DTOs.Sentences;

public class SentenceImportPreviewItemResponse
{
    public int RowNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
