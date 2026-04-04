using Domain.Entities;

namespace Application.IRepositories;

public interface IUserCardNoteRepository : IRepository<UserCardNote>
{
	Task<List<UserCardNote>> GetByCardIdWithRelationsAsync(string cardId);
	Task<(List<UserCardNote> Notes, int Total)> GetCommunityByCardIdAsync(string cardId, string currentUserId, int page, int pageSize);
	Task<UserCardNote?> GetMyNoteByCardIdAsync(string cardId, string userId);
	Task<UserCardNote?> GetByIdWithRelationsAsync(string noteId);
}
