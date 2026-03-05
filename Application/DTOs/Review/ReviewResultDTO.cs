namespace Application.DTOs.Review;

public class ReviewResultDTO
{
    public bool IsCorrect { set; get; }
    public string? ExpectedAnswer { set; get; }
    public int NewSrsLevel { set; get; }
    public DateTime? NextReviewAt { set; get; }
    public bool IsGhostEligible { set; get; }
}
