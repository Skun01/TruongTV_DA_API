using Application.DTOs.ExampleSentence;
using Application.IRepositories;
using Application.IServices;
using Domain.Constants;
using Domain.Entities;

namespace Application.Services;

public class ExampleSentenceService : IExampleSentenceService
{
    private readonly IUnitOfWork _unitOfWork;
    public ExampleSentenceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateExampleSentence(CreateCardExampleRequest request, string userId)
    {   
        var newExample = new ExampleSentence()
        {
            Id = Guid.NewGuid().ToString(),
            ClozeSentence = request.ClozeSentence,
            ExpectedAnswer = request.ExpectedAnswer,
            Hint = request.Hint
        };

        if(!string.IsNullOrEmpty(request.GrammarCardId))
        {
            var grammarCard = await _unitOfWork.GrammarCards.GetByIdAsync(request.GrammarCardId);

            if(grammarCard == null)
                throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);
            
            var deck = await _unitOfWork.Decks.GetByIdAsync(grammarCard.DeckId);
            
            if(deck?.CreatedBy != userId)
                throw new ApplicationException(MessageConstants.CommonMessage.NOT_ALLOW);

            newExample.GrammarCardId = request.GrammarCardId;

        }else
        {
            var vocabCard = await _unitOfWork.VocabularyCards.GetByIdAsync(request.VocabularyCardId!);
            
            if(vocabCard == null)
                throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

            var deck = await _unitOfWork.Decks.GetByIdAsync(vocabCard.DeckId);
            
            if(deck?.CreatedBy != userId)
                throw new ApplicationException(MessageConstants.CommonMessage.NOT_ALLOW);

            newExample.VocabularyCardId = request.VocabularyCardId;
        }

        await _unitOfWork.ExampleSentences.AddAsync(newExample);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateExampleAsync(UpdateCardExampleRequest request, string exampleId, string userId)
    {
        var example = await _unitOfWork.ExampleSentences.GetByIdAsync(exampleId);

        if(example == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        await ValidateOwnershipAsync(example, userId);

        example.ClozeSentence = request.ClozeSentence;
        example.ExpectedAnswer = request.ExpectedAnswer;
        example.Hint = request.Hint;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteExampleAsync(string id, string userId)
    {
        var example = await _unitOfWork.ExampleSentences.GetByIdAsync(id);

        if(example == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        await ValidateOwnershipAsync(example, userId);

        _unitOfWork.ExampleSentences.Delete(example);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Kiểm tra example có thuộc về user không
    private async Task ValidateOwnershipAsync(ExampleSentence example, string userId)
    {
        string? deckId = null;

        if(!string.IsNullOrEmpty(example.GrammarCardId))
        {
            var card = await _unitOfWork.GrammarCards.GetByIdAsync(example.GrammarCardId);
            deckId = card?.DeckId;
        }
        else if(!string.IsNullOrEmpty(example.VocabularyCardId))
        {
            var card = await _unitOfWork.VocabularyCards.GetByIdAsync(example.VocabularyCardId);
            deckId = card?.DeckId;
        }

        if(deckId == null)
            throw new ApplicationException(MessageConstants.CommonMessage.NOT_FOUND);

        var deck = await _unitOfWork.Decks.GetByIdAsync(deckId);

        if(deck?.CreatedBy != userId)
            throw new UnauthorizedAccessException(MessageConstants.CommonMessage.NOT_ALLOW);
    }
}
