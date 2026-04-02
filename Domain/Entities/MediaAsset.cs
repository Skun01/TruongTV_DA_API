using Domain.Enums;

namespace Domain.Entities;

public class MediaAsset : BaseEntity
{
    public string UserId { set; get; } = string.Empty;
    public string FileUrl { set; get; } = string.Empty;
    public string StorageKey { set; get; } = string.Empty;
    public string OriginalFileName { set; get; } = string.Empty;
    public string ContentType { set; get; } = string.Empty;
    public long SizeInBytes { set; get; }
    public FileType FileType { set; get; }
    public ResourceUsageType UsageType { set; get; }
    public StorageProvider StorageProvider { set; get; } = StorageProvider.Cloud;

    public User User { set; get; } = null!;
}
