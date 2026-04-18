using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface ICardRepository : IRepository<Card>
{
	Task<Card?> GetVocabularyDetailByIdAsync(string cardId);
	Task<Card?> GetGrammarDetailByIdAsync(string cardId);
	Task<Card?> GetKanjiDetailByIdAsync(string cardId);
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

	Task<(List<Card> Items, int Total)> SearchGrammarAsync(
		string? query,
		JlptLevel? level,
		PublishStatus? status,
		RegisterType? register,
		string? createdBy,
		int page,
		int pageSize);

	Task<(List<Card> Items, int Total)> SearchKanjiAsync(
		string? query,
		JlptLevel? level,
		PublishStatus? status,
		int? strokeCountMin,
		int? strokeCountMax,
		string? radical,
		string? createdBy,
		int page,
		int pageSize);

	Task<List<Card>> GetKanjiExportAsync(
		string? query,
		JlptLevel? level,
		PublishStatus? status,
		int? strokeCountMin,
		int? strokeCountMax,
		string? radical,
		string? createdBy);

	Task<bool> ExistsVocabularyByWritingAsync(string writing);
	Task<bool> ExistsKanjiByCharacterAsync(string kanji);

	Task<(List<Card> Items, int Total)> SearchCardsAsync(
		CardType? cardType,
		string? query,
		JlptLevel? level,
		int page,
		int pageSize);
	Task<Card?> GetLearningAdminCardByIdAsync(string cardId);
	Task<List<Card>> GetLearningAdminCardsByIdsAsync(List<string> cardIds);
	Task<List<Card>> SearchLearningAdminCardsAsync(
		CardType? cardType,
		string? query,
		List<string>? cardIds);
	Task<Card?> GetStudyCardByIdAsync(string cardId);
	Task<List<Card>> GetStudyCardsByIdsAsync(List<string> cardIds);
}
