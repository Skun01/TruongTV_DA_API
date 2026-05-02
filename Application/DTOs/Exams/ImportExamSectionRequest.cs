namespace Application.DTOs.Exams;

public class ImportExamSectionRequest
{
    public string SectionType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public List<ImportQuestionGroupRequest> QuestionGroups { get; set; } = new();
}
