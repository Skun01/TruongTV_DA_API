namespace Application.DTOs.LearningAdmin;

public class LearningAdminCardSentenceConfigResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Jp { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Level { get; set; }
    public string? BlankWord { get; set; }
    public string? Hint { get; set; }
    public List<string> AnswerList { get; set; } = new();
}
