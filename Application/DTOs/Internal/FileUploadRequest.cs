using Domain.Enums;

namespace Application.DTOs.Internal;

public class FileUploadRequest
{
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Stream Content { get; set; } = Stream.Null;
    public FileType FileType { get; set; }
    public ResourceUsageType UsageType { get; set; }
}
