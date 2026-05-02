namespace Application.DTOs.Exams;

public class ExamImportTemplateGuide
{
    public string JsonNamingConvention { get; set; } = "camelCase";
    public List<string> Overview { get; set; } = new();
    public Dictionary<string, List<string>> AllowedValues { get; set; } = new();
    public Dictionary<string, List<string>> RulesByNode { get; set; } = new();
}
