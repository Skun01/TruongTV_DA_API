namespace Application.DTOs.Exams;

public class PublishedExamDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int TotalDurationMinutes { get; set; }
    public int SectionsCount { get; set; }
    public int QuestionsCount { get; set; }
    public List<PublishedExamSectionSummaryResponse> Sections { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
