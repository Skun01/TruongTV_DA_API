namespace Application.DTOs.Grammar;

public class GrammarImportCommitResponse
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public bool HasValidationErrors { get; set; }
    public List<GrammarImportCommitItemResponse> Items { get; set; } = new();
}
