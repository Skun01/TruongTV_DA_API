using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface ICardRepository : IRepository<Card>
{
	Task<Card?> GetVocabularyDetailByIdAsync(string cardId);
	Task<(List<Card> Items, int Total)> SearchVocabularyAsync(
		string? query,
		JlptLevel? level,
		PublishStatus? status,
		string? createdBy,
		int page,
		int pageSize);
}
