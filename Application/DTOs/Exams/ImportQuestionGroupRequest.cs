namespace Application.DTOs.Exams;

public class ImportQuestionGroupRequest
{
    public string? PassageText { get; set; }
    public string? AudioUrl { get; set; }
    public string? AudioScript { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? MondaiType { get; set; }
    public List<ImportExamQuestionRequest> Questions { get; set; } = new();
}
