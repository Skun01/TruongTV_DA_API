namespace Application.DTOs.Vocabulary;

public class ImportVocabularyRequest
{
    public List<ImportVocabularyItemRequest> Items { get; set; } = new();
}
