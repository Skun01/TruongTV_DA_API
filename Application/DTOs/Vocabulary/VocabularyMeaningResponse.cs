namespace Application.DTOs.Vocabulary;

public class VocabularyMeaningResponse
{
    public string PartOfSpeech { get; set; } = string.Empty;
    public List<string> Definitions { get; set; } = new();
}
