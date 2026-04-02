namespace Application.IRepositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IMediaAssetRepository MediaAssets { get; }

    Task<int> SaveChangesAsync();
}
