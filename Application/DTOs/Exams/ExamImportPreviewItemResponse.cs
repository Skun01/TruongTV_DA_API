namespace Application.DTOs.Exams;

public class ExamImportPreviewItemResponse
{
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int SectionsCount { get; set; }
    public int QuestionGroupsCount { get; set; }
    public int QuestionsCount { get; set; }
    public int OptionsCount { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
