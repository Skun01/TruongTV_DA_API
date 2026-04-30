namespace Application.DTOs.ExamSessions;

public class SessionQuestionGroupResponse
{
    public string GroupId { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public string? AudioUrl { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? MondaiType { get; set; }
    public List<SessionQuestionResponse> Questions { get; set; } = new();
}
