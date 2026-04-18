namespace Application.DTOs.Learning;

public class StudyQuestionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? QuestionText { get; set; }
    public string? SecondaryText { get; set; }
    public string? Hint { get; set; }
    public string? FrontText { get; set; }
    public string? BackText { get; set; }
    public bool AllowsMultipleSelection { get; set; }
    public List<StudyQuestionOptionResponse> Options { get; set; } = new();
    public bool IsCompleted { get; set; }
}
