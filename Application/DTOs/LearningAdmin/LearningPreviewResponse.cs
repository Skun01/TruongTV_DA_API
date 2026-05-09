namespace Application.DTOs.LearningAdmin;

public class LearningPreviewResponse
{
    public string CardId { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? QuestionText { get; set; }
    public string? SecondaryText { get; set; }
    public string? Hint { get; set; }
    public string? FrontText { get; set; }
    public string? BackText { get; set; }
    public string? SentenceId { get; set; }
    public string QuestionSource { get; set; } = string.Empty;
    public int AcceptedAnswerCount { get; set; }
    public bool AllowsMultipleSelection { get; set; }
    public List<LearningPreviewOptionResponse> Options { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
