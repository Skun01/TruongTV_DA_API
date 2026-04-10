namespace Application.DTOs.Vocabulary;

public class ImportVocabularyItemRequest
{
    public int? RowNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Status { get; set; }

    public string Writing { get; set; } = string.Empty;
    public string? Reading { get; set; }
    public List<int>? PitchPattern { get; set; }
    public int? SpeakerId { get; set; }
    public string? WordType { get; set; }

    public List<VocabularyMeaningRequest> Meanings { get; set; } = new();
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<string> RelatedPhrases { get; set; } = new();
    public List<VocabularySentenceUpsertRequest> Sentences { get; set; } = new();
}
