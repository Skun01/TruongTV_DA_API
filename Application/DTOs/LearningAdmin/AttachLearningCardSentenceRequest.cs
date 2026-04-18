namespace Application.DTOs.LearningAdmin;

public class AttachLearningCardSentenceRequest
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string? BlankWord { get; set; }
    public string? Hint { get; set; }
    public List<string> AnswerList { get; set; } = new();
}
