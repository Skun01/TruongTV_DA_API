namespace Application.DTOs.Exams;

public class CreateSectionRequest
{
    public string SectionType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
}
