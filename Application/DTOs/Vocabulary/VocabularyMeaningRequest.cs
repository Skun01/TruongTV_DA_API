namespace Application.DTOs.Vocabulary;

public class VocabularyMeaningRequest
{
    public string PartOfSpeech { get; set; } = string.Empty;
    public List<string> Definitions { get; set; } = new();
}
