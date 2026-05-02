using System.Text.Json.Serialization;

namespace Application.DTOs.Exams;

public class ImportExamRequest
{
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int TotalDurationMinutes { get; set; }
    public List<ImportExamSectionRequest> Sections { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExamImportTemplateGuide? Guide { get; set; }
}
