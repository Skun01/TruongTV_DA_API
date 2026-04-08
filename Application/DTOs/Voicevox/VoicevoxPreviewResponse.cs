namespace Application.DTOs.Voicevox;

public class VoicevoxPreviewResponse
{
    public int SpeakerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
}
