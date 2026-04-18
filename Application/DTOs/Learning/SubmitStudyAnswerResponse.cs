namespace Application.DTOs.Learning;

public class SubmitStudyAnswerResponse
{
    public bool IsCorrect { get; set; }
    public string CardId { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public List<string> AcceptedAnswers { get; set; } = new();
    public string SrsLevel { get; set; } = string.Empty;
    public DateTime NextReviewAt { get; set; }
    public bool IsMastered { get; set; }
    public int ConsecutiveCorrect { get; set; }
    public int CompletedCards { get; set; }
    public int RemainingCards { get; set; }
}
