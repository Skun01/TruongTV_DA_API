using Domain.Entities;

namespace Application.IRepositories;

public interface ICardSentenceRepository : IRepository<CardSentence>
{
    Task<List<CardSentence>> GetByCardIdAsync(string cardId);
    Task<CardSentence?> GetByCardAndSentenceIdAsync(string cardId, string sentenceId);
}
