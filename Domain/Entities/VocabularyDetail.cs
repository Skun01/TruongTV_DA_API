using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class VocabularyDetail
{
    public string CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public string Writing { get; set; } = string.Empty;
    public string? Reading { get; set; }
    public string? PitchAccent { get; set; }
    public string? AudioUrl { get; set; }
    public int? SpeakerId { get; set; }
    public WordType? WordType { get; set; }
    
    // JSONB Mapping
    public List<MeaningItem> Meanings { get; set; } = new();
    
    // Array explicit mappings
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<string> RelatedPhrases { get; set; } = new();
}
