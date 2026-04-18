namespace Application.DTOs.Learning;

public class CreateStudySessionRequest
{
    public string DeckId { get; set; } = string.Empty;
    public List<string> CardIds { get; set; } = new();
    public string Mode { get; set; } = string.Empty;
    public StudySessionSettingsRequest? Settings { get; set; }
}
