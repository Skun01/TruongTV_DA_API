namespace Application.DTOs.LearningAdmin;

public class UpdateLearningCardConfigRequest
{
    public string Summary { get; set; } = string.Empty;
    public List<UpsertLearningCardSentenceConfigRequest> Sentences { get; set; } = new();
}
