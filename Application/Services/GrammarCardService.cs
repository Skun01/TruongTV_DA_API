using Application.DTOs.GrammarCard;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class GrammarCardService : IGrammarCardService
{
    private readonly IUnitOfWork _unitOfWork;
    public GrammarCardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateGrammarCardAsync(CreateGrammarCardRequest request, string userId)
    {
        var deck = await _unitOfWork.Decks.GetByIdAsync(request.DeckId);

        if(deck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(deck.CreatedBy != userId)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_ALLOW);

        if(deck.Type != DeckType.Grammar)
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var examples = request.Examples.Select(e => e.ToEntity()).ToList();

        var newGrammarCard = new GrammarCard()
        {
            Id = Guid.NewGuid().ToString(),
            Term = request.Term,
            Meaning = request.Meaning,
            Explanation = request.Explanation,
            Structure = request.Structure,
            Caution = request.Caution,
            DeckId = request.DeckId,
            ExampleSentences = examples
        };


        await _unitOfWork.GrammarCards.AddAsync(newGrammarCard);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteByIdAsync(string cardId, string userId)
    {
        var card = await _unitOfWork.GrammarCards.GetFullInfoByIdAsync(cardId);

        if(card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(card.Deck!.CreatedBy != userId)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_ALLOW);

        _unitOfWork.GrammarCards.Delete(card);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<GrammarCardDTO> GetCardByIdAsync(string id, string userId)
    {
        var card = await _unitOfWork.GrammarCards.GetFullInfoByIdAsync(id);

        if(card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(card.Deck!.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        return card.ToDTO();
    }

    public async Task<IEnumerable<GrammarCardDTO>> GetGrammarListByDeckIdAsync(string deckId, string userId)
    {
        var isDeckExist = await _unitOfWork.Decks.IsExist(deckId, DeckType.Grammar);

        if(!isDeckExist)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var cards = await _unitOfWork.GrammarCards.GetAllByDeckIdAsync(deckId);

        return cards.Select(c => c.ToDTO()).ToList();
    }

    public async Task<bool> UpdateCardByIdAsync(UpdateGrammarCardRequest request, string cardId, string userId)
    {
        var card = await _unitOfWork.GrammarCards.GetFullInfoByIdAsync(cardId);

        if(card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(card.Deck!.CreatedBy != userId)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_ALLOW);

        card.Term = request.Term;
        card.Meaning = request.Meaning;
        card.Explanation = request.Explanation;
        card.Caution = request.Caution;
        card.Structure = request.Structure;

        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}
