using Application.DTOs.Deck;
using Application.DTOs.ExampleSentence;
using Application.DTOs.Learn;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class LearnService : ILearnService
{
    private readonly IUnitOfWork _unitOfWork;
    public LearnService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LearnBatchDTO> GetLearnBatchAsync(string deckId, string userId)
    {
        var deck = await _unitOfWork.Decks.GetWithCardByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

        if (deck.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        // Lấy settings để biết batchSize
        var settings = await GetOrCreateSettingsAsync(userId);
        var batchSize = settings.BatchSize;

        // Lấy tất cả card IDs của deck
        var allCards = GetCardsFromDeck(deck);
        var allCardIds = allCards.Select(c => c.id).ToList();

        // Lấy progress đã có
        var existingProgresses = await _unitOfWork.CardProgresses
            .GetByUserAndCardIdsAsync(userId, allCardIds, deck.Type);
        var learnedCardIds = existingProgresses.Select(p => p.CardId).ToHashSet();

        // Tìm cards chưa learn
        var newCards = allCards.Where(c => !learnedCardIds.Contains(c.id)).ToList();
        var batch = newCards.Take(batchSize).ToList();

        // Map sang DTO
        var learnCards = batch.Select(card => MapToLearnCardDTO(card, deck.Type)).ToList();

        var dailyProgress = await BuildDailyProgressAsync(userId, settings);

        return new LearnBatchDTO
        {
            DeckId = deck.Id,
            DeckName = deck.Name,
            Cards = learnCards,
            TotalNewCards = newCards.Count,
            DailyProgress = dailyProgress
        };
    }

    public async Task<bool> MarkCardLearnedAsync(MarkCardRequest request, string userId)
    {
        // Kiểm tra card có tồn tại không
        var cardExists = request.CardType == DeckType.Vocabulary
            ? (await _unitOfWork.VocabularyCards.GetByIdAsync(request.CardId)) != null
            : (await _unitOfWork.GrammarCards.GetByIdAsync(request.CardId)) != null;

        if (!cardExists)
            throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

        // Kiểm tra đã learn chưa
        var existing = await _unitOfWork.CardProgresses
            .GetByUserAndCardAsync(userId, request.CardId, request.CardType);

        if (existing != null && existing.Status != CardStatus.New)
            throw new ApplicationException(MessageConstants.LearnMessage.ALREADY_LEARNED);

        if (existing == null)
        {
            var progress = new CardProgress
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                CardId = request.CardId,
                CardType = request.CardType,
                Status = request.IsMastered ? CardStatus.Mastered : CardStatus.Learning,
                SrsLevel = request.IsMastered ? 4 : 0,
                LearnedAt = DateTime.UtcNow,
                NextReviewAt = request.IsMastered
                    ? DateTime.UtcNow.AddDays(7)   // Mastered → 1 tuần
                    : DateTime.UtcNow.AddHours(4)   // Learning → 4 giờ (SRS Level 0)
            };

            await _unitOfWork.CardProgresses.AddAsync(progress);
        }
        else
        {
            // Card có status New → update
            existing.Status = request.IsMastered ? CardStatus.Mastered : CardStatus.Learning;
            existing.SrsLevel = request.IsMastered ? 4 : 0;
            existing.LearnedAt = DateTime.UtcNow;
            existing.NextReviewAt = request.IsMastered
                ? DateTime.UtcNow.AddDays(7)
                : DateTime.UtcNow.AddHours(4);
        }

        // Cập nhật streak
        await UpdateStreakAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DailyProgressDTO> GetDailyProgressAsync(string userId)
    {
        var settings = await GetOrCreateSettingsAsync(userId);
        return await BuildDailyProgressAsync(userId, settings);
    }

    public async Task<DeckProgressDTO> GetDeckProgressAsync(string deckId, string userId)
    {
        var deck = await _unitOfWork.Decks.GetWithCardByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

        if (deck.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        var allCards = GetCardsFromDeck(deck);
        var cardIds = allCards.Select(c => c.id).ToList();
        var totalCards = cardIds.Count;

        if (totalCards == 0)
            return new DeckProgressDTO();

        var learningCount = await _unitOfWork.CardProgresses
            .GetCountByStatusAsync(userId, cardIds, deck.Type, CardStatus.Learning);
        var masteredCount = await _unitOfWork.CardProgresses
            .GetCountByStatusAsync(userId, cardIds, deck.Type, CardStatus.Mastered);
        var dueCount = await _unitOfWork.CardProgresses
            .GetDueCountAsync(userId, cardIds, deck.Type);
        var (correct, total) = await _unitOfWork.CardProgresses
            .GetAccuracyAsync(userId, cardIds, deck.Type);

        var newCards = totalCards - learningCount - masteredCount;

        return new DeckProgressDTO
        {
            TotalCards = totalCards,
            NewCards = newCards,
            LearningCards = learningCount,
            MasteredCards = masteredCount,
            DueForReview = dueCount,
            AccuracyPercent = total > 0 ? Math.Round((double)correct / total * 100, 1) : 0
        };
    }

    // ========== Private Helpers ==========

    private async Task UpdateStreakAsync(string userId)
    {
        var settings = await GetOrCreateSettingsAsync(userId);
        var todayCount = await _unitOfWork.CardProgresses.GetTodayLearnedCountAsync(userId);

        if (todayCount >= settings.DailyGoal)
        {
            var todayUtc = DateTime.UtcNow.Date;

            if (settings.LastStudyDate?.Date == todayUtc)
                return; // Đã tính streak hôm nay rồi

            if (settings.LastStudyDate?.Date == todayUtc.AddDays(-1))
                settings.CurrentStreak += 1; // Tiếp tục streak
            else
                settings.CurrentStreak = 1; // Reset streak mới

            if (settings.CurrentStreak > settings.LongestStreak)
                settings.LongestStreak = settings.CurrentStreak;

            settings.LastStudyDate = todayUtc;
        }
    }

    private async Task<DailyProgressDTO> BuildDailyProgressAsync(string userId, UserSettings settings)
    {
        var learnedToday = await _unitOfWork.CardProgresses.GetTodayLearnedCountAsync(userId);
        return new DailyProgressDTO
        {
            LearnedToday = learnedToday,
            DailyGoal = settings.DailyGoal,
            IsGoalReached = learnedToday >= settings.DailyGoal,
            CurrentStreak = settings.CurrentStreak
        };
    }

    private async Task<UserSettings> GetOrCreateSettingsAsync(string userId)
    {
        var settings = await _unitOfWork.UserSettings.GetByUserIdAsync(userId);
        if (settings == null)
        {
            settings = new UserSettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId
            };
            await _unitOfWork.UserSettings.AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();
        }
        return settings;
    }

    /// <summary>
    /// Trích xuất (id, card object) từ deck tuỳ theo loại
    /// </summary>
    private static List<(string id, object card)> GetCardsFromDeck(Deck deck)
    {
        return deck.Type == DeckType.Vocabulary
            ? deck.VocabularyCards.Select(c => (c.Id, (object)c)).ToList()
            : deck.GrammarCards.Select(c => (c.Id, (object)c)).ToList();
    }

    private static LearnCardDTO MapToLearnCardDTO((string id, object card) item, DeckType type)
    {
        if (type == DeckType.Vocabulary && item.card is VocabularyCard vocab)
        {
            return new LearnCardDTO
            {
                CardId = vocab.Id,
                CardType = DeckType.Vocabulary,
                Term = vocab.Term,
                Meaning = vocab.Meaning,
                HasExamples = vocab.ExampleSentences.Any(),
                Examples = vocab.ExampleSentences.Select(e => e.ToDTO())
            };
        }
        else if (type == DeckType.Grammar && item.card is GrammarCard grammar)
        {
            return new LearnCardDTO
            {
                CardId = grammar.Id,
                CardType = DeckType.Grammar,
                Term = grammar.Term,
                Meaning = grammar.Meaning,
                Structure = grammar.Structure,
                Explanation = grammar.Explanation,
                Caution = grammar.Caution,
                HasExamples = grammar.ExampleSentences.Any(),
                Examples = grammar.ExampleSentences.Select(e => e.ToDTO())
            };
        }

        throw new ApplicationException(MessageConstants.CommonMessage.INVALID);
    }
}
