namespace Application.DTOs.ExamSessions;

public class SubmitSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int UnansweredCount { get; set; }
    public bool IsPassed { get; set; }
    public List<SectionScoreResponse> SectionScores { get; set; } = new();
}
