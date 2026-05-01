namespace Application.DTOs.Exams;

public class PublishedExamListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int TotalDurationMinutes { get; set; }
    public int SectionsCount { get; set; }
    public int QuestionsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
