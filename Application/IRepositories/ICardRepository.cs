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
		WordType? wordType,
		bool? hasAudio,
		string? createdBy,
		int page,
		int pageSize);

	Task<List<Card>> GetVocabularyExportAsync(
		string? query,
		JlptLevel? level,
		PublishStatus? status,
		WordType? wordType,
		bool? hasAudio,
		string? createdBy);

	Task<bool> ExistsVocabularyByWritingAsync(string writing);

	Task<(List<Card> Items, int Total)> SearchCardsAsync(
		CardType? cardType,
		string? query,
		JlptLevel? level,
		int page,
		int pageSize);
}
