using Application.DTOs.Review;

namespace Application.IServices;

public interface IReviewService
{
    Task<ReviewSessionDTO> GetReviewSessionAsync(string deckId, string userId);
    Task<ReviewResultDTO> SubmitReviewAsync(SubmitReviewRequest request, string userId);
}
