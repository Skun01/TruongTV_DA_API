namespace Application.DTOs.Learning;

public class SubmitStudyAnswerRequest
{
    public string CardId { get; set; } = string.Empty;
    public List<string> Answers { get; set; } = new();
    public List<string> SelectedOptionIds { get; set; } = new();
    public string? FlashcardResult { get; set; }
}
