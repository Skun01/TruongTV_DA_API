namespace Application.DTOs.Grammar;

public class GrammarImportPreviewResponse
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int InvalidItems { get; set; }
    public List<GrammarImportPreviewItemResponse> Items { get; set; } = new();
}
