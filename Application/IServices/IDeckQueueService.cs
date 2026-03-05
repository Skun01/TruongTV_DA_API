using Application.DTOs.DeckQueue;

namespace Application.IServices;

public interface IDeckQueueService
{
    Task<IEnumerable<DeckQueueDTO>> GetQueueAsync(string userId);
    Task<bool> AddToQueueAsync(string deckId, string userId);
    Task<bool> RemoveFromQueueAsync(string deckId, string userId);
}
