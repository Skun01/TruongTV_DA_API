using Domain.Enums;

namespace Domain.Entities;

public class Sentence : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int? SpeakerId { get; set; }
    public JlptLevel? Level { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation
    public ICollection<CardSentence> CardSentences { get; set; } = new List<CardSentence>();
}
