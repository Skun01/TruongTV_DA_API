namespace Application.DTOs.LearningAdmin;

public class DeckLearningCoverageCardTypeResponse
{
    public string CardType { get; set; } = string.Empty;
    public int Total { get; set; }
    public int FillInBlankReady { get; set; }
    public int MultipleChoiceReady { get; set; }
    public int FlashcardReady { get; set; }
}
