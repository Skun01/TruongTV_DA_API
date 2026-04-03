using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class UserCardNoteRepository : Repository<UserCardNote>, IUserCardNoteRepository
{
    public UserCardNoteRepository(AppDbContext context) : base(context)
    {
    }
}
