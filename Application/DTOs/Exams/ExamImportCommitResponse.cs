namespace Application.DTOs.Exams;

public class ExamImportCommitResponse
{
    public bool IsSuccess { get; set; }
    public bool HasValidationErrors { get; set; }
    public string Action { get; set; } = "skipped";
    public string Title { get; set; } = string.Empty;
    public string? ExamId { get; set; }
    public int SectionsCount { get; set; }
    public int QuestionGroupsCount { get; set; }
    public int QuestionsCount { get; set; }
    public int OptionsCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
