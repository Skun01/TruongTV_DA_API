using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class CardSentenceRepository : Repository<CardSentence>, ICardSentenceRepository
{
    public CardSentenceRepository(AppDbContext context) : base(context)
    {
    }
}
