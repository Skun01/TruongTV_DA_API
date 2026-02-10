using Application.DTOs.Common;
using Application.DTOs.Deck;
using Application.DTOs.User;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DeckService : IDeckService
{
    private readonly IUnitOfWork _unitOfWork;
    public DeckService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateDeckAsync(CreateDeckRequest request, string userId)
    {
        var newDeck = new Deck()
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            CreatedBy = userId
        };

        await _unitOfWork.Decks.AddAsync(newDeck);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteDeckAsync(string id, string userId)
    {
        var deck = await _unitOfWork.Decks.GetByIdAsync(id);
        
        if(deck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(deck.CreatedBy != userId)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_ALLOW);

        _unitOfWork.Decks.Delete(deck);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DeckDTO> GetDeckSummaryContent(string id)
    {
        var deck = await _unitOfWork.Decks.GetWithCardByIdAsync(id);
        
        if(deck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);
        
        var response = new DeckDTO
        {
            Id = deck.Id,
            Name = deck.Name,
            Description = deck.Description,
            Type = deck.Type,
            Author = new UserDTO
            {
                Id = deck.User!.Id,
                Username = deck.User.Username
            },
            Cards = deck.Type == DeckType.Vocabulary
                ? deck.VocabularyCards.Select(c => c.ToPreviewDTO()).ToList()
                : deck.GrammarCards.Select(c => c.ToPreviewDTO()).ToList()
        };

        return response;
    }

    public async Task<IEnumerable<DeckSummaryDTO>> GetMyDeckByFilterAsync(QueryDTO<SearchDeckQueryDTO> query, string userId)
    {
        var decks = await _unitOfWork.Decks.GetByFilterAsync(query, userId);

        return decks.Select(d => d.ToSummaryDTO());
    }

    public async Task<bool> UpdateDeckAsync(UpdateDeckRequest request, string userId, string deckId)
    {
        var deck = await _unitOfWork.Decks.GetByIdAsync(deckId);

        if(deck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(deck.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        deck.Name = request.Name;
        deck.Description = request.Description;
        deck.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
