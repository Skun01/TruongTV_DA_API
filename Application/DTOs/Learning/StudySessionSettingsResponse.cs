namespace Application.DTOs.Learning;

public class StudySessionSettingsResponse
{
    public string FlashcardFront { get; set; } = string.Empty;
    public string FlashcardBack { get; set; } = string.Empty;
    public string MultipleChoiceQuestion { get; set; } = string.Empty;
    public bool ShuffleOptions { get; set; }
}
