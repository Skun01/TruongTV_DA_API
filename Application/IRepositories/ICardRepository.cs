using Domain.Entities;

namespace Application.IRepositories;

public interface ICardRepository : IRepository<Card>
{
	Task<Card?> GetVocabularyDetailByIdAsync(string cardId);
}
