namespace Application.DTOs.Exams;

public class ExamImportPreviewResponse
{
    public bool IsValid { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public ExamImportPreviewItemResponse Item { get; set; } = new();
}
