namespace Application.DTOs.Auth;

public class UploadAvatarRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public Stream Content { get; set; } = Stream.Null;
}
