namespace Application.DTOs.LearningAdmin;

public class ReorderLearningCardSentenceItemRequest
{
    public string SentenceId { get; set; } = string.Empty;
    public int Position { get; set; }
}
