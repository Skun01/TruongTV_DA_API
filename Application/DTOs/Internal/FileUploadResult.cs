using Domain.Enums;

namespace Application.DTOs.Internal;

public class FileUploadResult
{
    public string StorageKey { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public FileType FileType { get; set; }
    public ResourceUsageType UsageType { get; set; }
}
