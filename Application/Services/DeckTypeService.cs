using Application.DTOs.Decks;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;

namespace Application.Services;

public class DeckTypeService : IDeckTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public DeckTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DeckTypeResponse>> GetAllAsync()
    {
        var deckTypes = await _unitOfWork.DeckTypes.GetAllOrderedAsync();
        return deckTypes.Select(dt => dt.ToResponse()).ToList();
    }
}
