namespace Application.DTOs.Sentences;

public class SentenceImportPreviewResponse
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int InvalidItems { get; set; }
    public List<SentenceImportPreviewItemResponse> Items { get; set; } = new();
}
