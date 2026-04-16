using Application.DTOs.Decks;

namespace Application.IServices;

public interface IDeckTypeService
{
    Task<List<DeckTypeResponse>> GetAllAsync();
}
