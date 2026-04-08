namespace Application.Settings;

public class VoicevoxSettings
{
    public const string SectionName = "VoicevoxConfig";

    public string BaseUrl { get; set; } = "http://localhost:50021";
    public string StoragePath { get; set; } = "wwwroot/audio-cache";
    public int Speaker { get; set; } = 1;
}