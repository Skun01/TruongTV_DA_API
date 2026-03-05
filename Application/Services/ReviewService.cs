using Application.DTOs.Review;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    // SRS intervals: Level → TimeSpan
    private static readonly TimeSpan[] SrsIntervals =
    [
        TimeSpan.FromHours(4),     // Level 0
        TimeSpan.FromHours(8),     // Level 1
        TimeSpan.FromDays(1),      // Level 2
        TimeSpan.FromDays(3),      // Level 3
        TimeSpan.FromDays(7),      // Level 4
        TimeSpan.FromDays(14),     // Level 5
        TimeSpan.FromDays(30),     // Level 6
        TimeSpan.FromDays(60),     // Level 7
    ];

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReviewSessionDTO> GetReviewSessionAsync(string deckId, string userId)
    {
        var deck = await _unitOfWork.Decks.GetWithCardByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

        if (deck.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        // Lấy tất cả card IDs
        var cardIds = deck.Type == DeckType.Vocabulary
            ? deck.VocabularyCards.Select(c => c.Id).ToList()
            : deck.GrammarCards.Select(c => c.Id).ToList();

        // Tìm CardProgress đã due
        var dueProgresses = await _unitOfWork.CardProgresses
            .GetDueForReviewAsync(userId, cardIds, deck.Type);

        // Build review items
        var items = new List<ReviewItemDTO>();
        foreach (var progress in dueProgresses)
        {
            var item = await BuildReviewItemAsync(progress, deck);
            if (item != null)
                items.Add(item);
        }

        return new ReviewSessionDTO
        {
            DeckId = deck.Id,
            DeckName = deck.Name,
            TotalDue = items.Count,
            Items = items
        };
    }

    public async Task<ReviewResultDTO> SubmitReviewAsync(SubmitReviewRequest request, string userId)
    {
        var progress = await _unitOfWork.CardProgresses.GetByIdAsync(request.CardProgressId);
        if (progress == null)
            throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

        if (progress.UserId != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        // Xác định đúng/sai
        bool isCorrect;
        string? expectedAnswer = null;

        if (request.ExampleSentenceId != null)
        {
            // Cloze mode: so sánh đáp án
            var example = await _unitOfWork.ExampleSentences.GetByIdAsync(request.ExampleSentenceId);
            if (example == null)
                throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

            expectedAnswer = example.ExpectedAnswer;
            isCorrect = CompareAnswers(request.UserAnswer, example.ExpectedAnswer);
        }
        else
        {
            // Flashcard mode: self-assessment
            isCorrect = request.IsCorrect ?? false;
        }

        // Ghost review: chỉ ghi log, KHÔNG thay đổi SRS
        if (request.IsGhost)
        {
            await CreateReviewLogAsync(userId, request, progress.Id, isCorrect);
            await _unitOfWork.SaveChangesAsync();

            return new ReviewResultDTO
            {
                IsCorrect = isCorrect,
                ExpectedAnswer = expectedAnswer,
                NewSrsLevel = progress.SrsLevel,
                NextReviewAt = progress.NextReviewAt,
                IsGhostEligible = !isCorrect
            };
        }

        // Cập nhật SRS
        int newLevel;
        if (isCorrect)
        {
            progress.CorrectStreak += 1;
            progress.CorrectReviews += 1;
            newLevel = Math.Min(progress.SrsLevel + 1, SrsIntervals.Length - 1);
        }
        else
        {
            progress.CorrectStreak = 0;
            newLevel = Math.Max(progress.SrsLevel - 2, 0);
        }

        progress.SrsLevel = newLevel;
        progress.TotalReviews += 1;
        progress.LastReviewedAt = DateTime.UtcNow;
        progress.NextReviewAt = DateTime.UtcNow.Add(SrsIntervals[newLevel]);

        // Cập nhật NextExampleIndex (chỉ cho Cloze mode)
        if (request.ExampleSentenceId != null)
        {
            progress.NextExampleIndex += 1;
        }

        // Ghi review log
        await CreateReviewLogAsync(userId, request, progress.Id, isCorrect);
        await _unitOfWork.SaveChangesAsync();

        return new ReviewResultDTO
        {
            IsCorrect = isCorrect,
            ExpectedAnswer = expectedAnswer,
            NewSrsLevel = newLevel,
            NextReviewAt = progress.NextReviewAt,
            IsGhostEligible = !isCorrect && !request.IsGhost
        };
    }

    // ========== Private Helpers ==========

    private async Task<ReviewItemDTO?> BuildReviewItemAsync(CardProgress progress, Deck deck)
    {
        if (progress.CardType == DeckType.Vocabulary)
        {
            var card = deck.VocabularyCards.FirstOrDefault(c => c.Id == progress.CardId);
            if (card == null) return null;

            var examples = card.ExampleSentences.OrderBy(e => e.CreatedAt).ToList();
            if (examples.Count > 0)
            {
                var index = progress.NextExampleIndex % examples.Count;
                var example = examples[index];
                return new ReviewItemDTO
                {
                    CardProgressId = progress.Id,
                    ExampleSentenceId = example.Id,
                    ReviewType = "Cloze",
                    ClozeSentence = example.ClozeSentence,
                    Hint = example.Hint,
                    CardTerm = card.Term,
                    CardMeaning = card.Meaning,
                    SrsLevel = progress.SrsLevel
                };
            }

            // Flashcard mode: card không có example
            return new ReviewItemDTO
            {
                CardProgressId = progress.Id,
                ExampleSentenceId = null,
                ReviewType = "Flashcard",
                CardTerm = card.Term,
                CardMeaning = card.Meaning,
                SrsLevel = progress.SrsLevel
            };
        }
        else
        {
            var card = deck.GrammarCards.FirstOrDefault(c => c.Id == progress.CardId);
            if (card == null) return null;

            var examples = card.ExampleSentences.OrderBy(e => e.CreatedAt).ToList();
            if (examples.Count > 0)
            {
                var index = progress.NextExampleIndex % examples.Count;
                var example = examples[index];
                return new ReviewItemDTO
                {
                    CardProgressId = progress.Id,
                    ExampleSentenceId = example.Id,
                    ReviewType = "Cloze",
                    ClozeSentence = example.ClozeSentence,
                    Hint = example.Hint,
                    CardTerm = card.Term,
                    CardMeaning = card.Meaning,
                    SrsLevel = progress.SrsLevel
                };
            }

            return new ReviewItemDTO
            {
                CardProgressId = progress.Id,
                ExampleSentenceId = null,
                ReviewType = "Flashcard",
                CardTerm = card.Term,
                CardMeaning = card.Meaning,
                SrsLevel = progress.SrsLevel
            };
        }
    }

    private async Task CreateReviewLogAsync(string userId, SubmitReviewRequest request, string cardProgressId, bool isCorrect)
    {
        var log = new ReviewLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ExampleSentenceId = request.ExampleSentenceId,
            CardProgressId = cardProgressId,
            IsCorrect = isCorrect,
            UserAnswer = request.UserAnswer,
            IsGhost = request.IsGhost
        };
        await _unitOfWork.ReviewLogs.AddAsync(log);
    }

    private static bool CompareAnswers(string? userAnswer, string expectedAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer))
            return false;

        return string.Equals(
            userAnswer.Trim(),
            expectedAnswer.Trim(),
            StringComparison.OrdinalIgnoreCase
        );
    }
}
