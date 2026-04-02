using Domain.Entities;
using Domain.Enums;

namespace Application.IRepositories;

public interface IMediaAssetRepository : IRepository<MediaAsset>
{
    Task<MediaAsset?> GetLatestByUserAndUsageAsync(string userId, ResourceUsageType usageType);
}
