namespace Application.DTOs.Sentences;

public class SentenceImportCommitResponse
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public bool HasValidationErrors { get; set; }
    public List<SentenceImportCommitItemResponse> Items { get; set; } = new();
}
