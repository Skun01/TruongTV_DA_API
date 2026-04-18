namespace Application.DTOs.LearningAdmin;

public class UpsertLearningCardSentenceConfigRequest
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string? BlankWord { get; set; }
    public string? Hint { get; set; }
    public List<string> AnswerList { get; set; } = new();
}
