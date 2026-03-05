namespace Application.DTOs.Review;

public class SubmitReviewRequest
{
    public string CardProgressId { set; get; } = string.Empty;
    public string? ExampleSentenceId { set; get; }
    public string? UserAnswer { set; get; }
    public bool? IsCorrect { set; get; }  // Required for Flashcard mode (self-assessment)
    public bool IsGhost { set; get; } = false;
}
