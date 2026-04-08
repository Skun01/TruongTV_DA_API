namespace Application.DTOs.Internal;

public class VoicevoxSynthesisResult
{
    public string AudioUrl { get; set; } = string.Empty;
    public int SpeakerId { get; set; }
    public List<int>? PitchPattern { get; set; }
}
