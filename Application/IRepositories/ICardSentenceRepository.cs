using Domain.Entities;

namespace Application.IRepositories;

public interface ICardSentenceRepository : IRepository<CardSentence>
{
    Task<List<CardSentence>> GetByCardIdAsync(string cardId);
}
