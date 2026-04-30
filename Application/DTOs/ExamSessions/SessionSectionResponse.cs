namespace Application.DTOs.ExamSessions;

/// <summary>
/// Section trong bài thi — không chứa đáp án đúng
/// </summary>
public class SessionSectionResponse
{
    public string SectionId { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int DurationMinutes { get; set; }
    public List<SessionQuestionGroupResponse> QuestionGroups { get; set; } = new();
}
