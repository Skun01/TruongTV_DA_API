namespace Application.DTOs.LearningAdmin;

public class DeckLearningCoverageResponse
{
    public string DeckId { get; set; } = string.Empty;
    public string DeckTitle { get; set; } = string.Empty;
    public int TotalCards { get; set; }
    public int FillInBlankReadyCount { get; set; }
    public int MultipleChoiceReadyCount { get; set; }
    public int FlashcardReadyCount { get; set; }
    public int IssueCount { get; set; }
    public List<DeckLearningCoverageCardTypeResponse> CardsByType { get; set; } = new();
}
