namespace Application.DTOs.Shadowing;

public class SubmitShadowingAttemptRequest
{
    public string TopicId { get; set; } = string.Empty;
    public string SentenceId { get; set; } = string.Empty;
    public string Locale { get; set; } = "ja-JP";
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public byte[] AudioBytes { get; set; } = [];
}
