namespace Application.DTOs.Exams;

public class ImportExamQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageCaption { get; set; }
    public string? Explanation { get; set; }
    public int Score { get; set; } = 1;
    public int OrderIndex { get; set; }
    public List<ImportQuestionOptionRequest> Options { get; set; } = new();
}
