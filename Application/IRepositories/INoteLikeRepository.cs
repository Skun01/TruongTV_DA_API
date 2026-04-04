using Domain.Entities;

namespace Application.IRepositories;

public interface INoteLikeRepository : IRepository<NoteLike>
{
	Task<NoteLike?> GetByUserAndNoteIdAsync(string userId, string noteId);
}
