using Application.DTOs.ExampleSentence;

namespace Application.DTOs.GrammarCard;

public class GrammarCardDTO
{
    public string Id { set; get; } = string.Empty;
    public string Term { set; get; } = string.Empty;
    public string Meaning { set; get; } = string.Empty;
    public string Structure { set; get; } = string.Empty;
    public string? Explanation { set; get; }
    public string? Caution { set; get; }
    public string? DeckId { set; get; }
     public IEnumerable<ExampleSentenceDTO> Examples { set; get; } = [];
}
