namespace Application.DTOs.Grammar;

public class GrammarImportPreviewItemResponse
{
    public int RowNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
