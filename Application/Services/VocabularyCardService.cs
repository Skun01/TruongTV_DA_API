using Application.DTOs.VocabularyCard;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class VocabularyCardService : IVocabularyCardService
{
    private readonly IUnitOfWork _unitOfWork;
    public VocabularyCardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateVocabularyCardAsync(CreateVocabularyRequest request, string userId)
    {
        var deck = await _unitOfWork.Decks.GetByIdAsync(request.DeckId);

        if(deck == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(deck.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        if(deck.Type != DeckType.Vocabulary)
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var examples = request.Examples.Select(e => e.ToEntity()).ToList();

        var newVocabCard = new VocabularyCard()
        {
            Id = Guid.NewGuid().ToString(),
            Term = request.Term,
            Meaning = request.Meaning,
            DeckId = request.DeckId,
            ExampleSentences = examples
        };
        
        await _unitOfWork.VocabularyCards.AddAsync(newVocabCard);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteByIdAsync(string id, string userId)
    {
        var card = await _unitOfWork.VocabularyCards.GetFullInfoByIdAsync(id);

        if(card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(card.Deck!.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        _unitOfWork.VocabularyCards.Delete(card);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<VocabularyCardDTO> GetCardByIdAsync(string id, string userId)
    {
        var card = await _unitOfWork.VocabularyCards.GetFullInfoByIdAsync(id);

        if(card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(card.Deck!.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        return card.ToDTO();
    }

    public async Task<IEnumerable<VocabularyCardDTO>> GetVocabularyListByDeckIdAsync(string deckId)
    {
        var isDeckExist = await _unitOfWork.Decks.IsExist(deckId, DeckType.Vocabulary);

        if(!isDeckExist)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var cards = await _unitOfWork.VocabularyCards.GetAllByDeckId(deckId);

        return cards.Select(c => c.ToDTO()).ToList();
    }

    public async Task<bool> UpdateCardByIdAsync(UpdateVocabularyCardRequest request, string cardId, string userId)
    {
        var card = await _unitOfWork.VocabularyCards.GetFullInfoByIdAsync(cardId);

        if(card == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        if(card.Deck!.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);

        card.Term = request.Term;
        card.Meaning = request.Meaning;

        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}
