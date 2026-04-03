using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class NoteLikeRepository : Repository<NoteLike>, INoteLikeRepository
{
    public NoteLikeRepository(AppDbContext context) : base(context)
    {
    }
}
