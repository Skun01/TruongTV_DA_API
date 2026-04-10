namespace Application.DTOs.Sentences;

public class ImportSentenceRequest
{
    public List<ImportSentenceItemRequest> Items { get; set; } = new();
}
