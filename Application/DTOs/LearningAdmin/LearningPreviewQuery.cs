namespace Application.DTOs.LearningAdmin;

public class LearningPreviewQuery
{
    public string Mode { get; set; } = string.Empty;
    public string? MultipleChoiceQuestion { get; set; }
    public string? FlashcardFront { get; set; }
    public string? FlashcardBack { get; set; }
    public bool? ShuffleOptions { get; set; }
}
