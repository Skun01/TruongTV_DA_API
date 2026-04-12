using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RadicalDetailRepository : Repository<RadicalDetail>, IRadicalDetailRepository
{
    public RadicalDetailRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RadicalDetail?> GetByCharacterAsync(string character)
    {
        var normalizedCharacter = character.Trim();

        return await _context.RadicalDetails
            .FirstOrDefaultAsync(r => r.Character == normalizedCharacter);
    }
}
