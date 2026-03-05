using Application.DTOs.DeckQueue;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DeckQueueService : IDeckQueueService
{
    private readonly IUnitOfWork _unitOfWork;
    public DeckQueueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DeckQueueDTO>> GetQueueAsync(string userId)
    {
        var queues = await _unitOfWork.DeckQueues.GetByUserIdAsync(userId);
        var result = new List<DeckQueueDTO>();

        foreach (var q in queues)
        {
            var deck = q.Deck;
            var cardIds = GetCardIds(deck);
            var totalCards = cardIds.Count;

            var learnedCount = 0;
            var dueCount = 0;

            if (totalCards > 0)
            {
                var progresses = await _unitOfWork.CardProgresses
                    .GetByUserAndCardIdsAsync(userId, cardIds, deck.Type);
                learnedCount = progresses.Count;
                dueCount = await _unitOfWork.CardProgresses
                    .GetDueCountAsync(userId, cardIds, deck.Type);
            }

            result.Add(new DeckQueueDTO
            {
                DeckId = deck.Id,
                DeckName = deck.Name,
                DeckType = deck.Type,
                TotalCards = totalCards,
                LearnedCards = learnedCount,
                DueForReview = dueCount,
                AddedAt = q.CreatedAt
            });
        }

        return result;
    }

    public async Task<bool> AddToQueueAsync(string deckId, string userId)
    {
        var deck = await _unitOfWork.Decks.GetByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException(MessageConstants.CommonMessage.NOT_FOUND);

        if (deck.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        if (await _unitOfWork.DeckQueues.IsExist(userId, deckId))
            throw new ApplicationException(MessageConstants.LearnMessage.ALREADY_IN_QUEUE);

        var queueItem = new DeckQueue
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            DeckId = deckId
        };

        await _unitOfWork.DeckQueues.AddAsync(queueItem);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFromQueueAsync(string deckId, string userId)
    {
        var queueItem = await _unitOfWork.DeckQueues.GetByUserAndDeckAsync(userId, deckId);
        if (queueItem == null)
            throw new ApplicationException(MessageConstants.LearnMessage.NOT_IN_QUEUE);

        _unitOfWork.DeckQueues.Delete(queueItem);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static List<string> GetCardIds(Deck deck)
    {
        return deck.Type == DeckType.Vocabulary
            ? deck.VocabularyCards.Select(c => c.Id).ToList()
            : deck.GrammarCards.Select(c => c.Id).ToList();
    }
}
