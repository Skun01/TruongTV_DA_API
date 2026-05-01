namespace Application.DTOs.Exams;

public class PublishedExamSectionSummaryResponse
{
    public string SectionId { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int DurationMinutes { get; set; }
    public int QuestionGroupsCount { get; set; }
    public int QuestionsCount { get; set; }
}
