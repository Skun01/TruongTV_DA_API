namespace Application.DTOs.Vocabulary;

public class VocabularyImportCommitResponse
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public bool HasValidationErrors { get; set; }
    public List<VocabularyImportCommitItemResponse> Items { get; set; } = new();
}
