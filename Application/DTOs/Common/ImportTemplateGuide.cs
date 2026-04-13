namespace Application.DTOs.Common;

public class ImportTemplateGuide
{
    public string JsonNamingConvention { get; set; } = "camelCase";
    public Dictionary<string, List<string>> AllowedValues { get; set; } = new();
    public Dictionary<string, string> FieldNotes { get; set; } = new();
}
