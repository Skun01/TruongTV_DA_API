namespace Application.DTOs.LearningAdmin;

public class LearningAdminCardConfigResponse
{
    public string CardId { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsFillInBlankReady { get; set; }
    public bool IsMultipleChoiceReady { get; set; }
    public bool IsFlashcardReady { get; set; }
    public List<string> AvailableModes { get; set; } = new();
    public List<LearningAdminCardIssueItemResponse> Issues { get; set; } = new();
    public List<LearningAdminCardSentenceConfigResponse> Sentences { get; set; } = new();
}
