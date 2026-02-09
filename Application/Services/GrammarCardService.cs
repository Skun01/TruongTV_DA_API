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
}
