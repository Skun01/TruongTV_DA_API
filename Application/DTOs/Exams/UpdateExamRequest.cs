namespace Application.DTOs.Exams;

public class UpdateExamRequest
{
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int TotalDurationMinutes { get; set; }
}
