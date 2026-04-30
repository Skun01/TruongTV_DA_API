namespace Application.DTOs.Exams;

public class ExamSectionResponse
{
    public string Id { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public int QuestionGroupsCount { get; set; }
    public int QuestionsCount { get; set; }
    public List<QuestionGroupResponse> QuestionGroups { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
