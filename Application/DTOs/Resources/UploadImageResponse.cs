namespace Application.DTOs.Resources;

public class UploadImageResponse
{
    public string Id { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string UsageType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
