namespace Application.DTOs.ShadowingAdmin;

public class AdminShadowingAvailableSentenceResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int? SpeakerId { get; set; }
    public string? Level { get; set; }
    public bool IsAttached { get; set; }
    public int? AttachedPosition { get; set; }
    public string? AttachedNote { get; set; }
}
