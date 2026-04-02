using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MediaAssetRepository : Repository<MediaAsset>, IMediaAssetRepository
{
    public MediaAssetRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<MediaAsset?> GetLatestByUserAndUsageAsync(string userId, ResourceUsageType usageType)
    {
        return await _context.MediaAssets
            .Where(ma => ma.UserId == userId && ma.UsageType == usageType)
            .OrderByDescending(ma => ma.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
