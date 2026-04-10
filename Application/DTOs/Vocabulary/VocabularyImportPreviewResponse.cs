namespace Application.DTOs.Vocabulary;

public class VocabularyImportPreviewResponse
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int InvalidItems { get; set; }
    public List<VocabularyImportPreviewItemResponse> Items { get; set; } = new();
}
