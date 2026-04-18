namespace Application.DTOs.Learning;

public class StudySessionSettingsRequest
{
    public string? FlashcardFront { get; set; }
    public string? FlashcardBack { get; set; }
    public string? MultipleChoiceQuestion { get; set; }
    public bool? ShuffleOptions { get; set; }
}
