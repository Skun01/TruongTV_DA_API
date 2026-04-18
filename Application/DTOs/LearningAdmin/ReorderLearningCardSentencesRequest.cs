namespace Application.DTOs.LearningAdmin;

public class ReorderLearningCardSentencesRequest
{
    public List<ReorderLearningCardSentenceItemRequest> Items { get; set; } = new();
}
