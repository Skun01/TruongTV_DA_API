using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class ReviewLogRepository : Repository<ReviewLog>, IReviewLogRepository
{
    public ReviewLogRepository(AppDbContext context) : base(context) {}
}
