using Application.DTOs.ExampleSentence;
using Domain.Enums;

namespace Application.DTOs.Learn;

public class LearnCardDTO
{
    public string CardId { set; get; } = string.Empty;
    public DeckType CardType { set; get; }
    public string Term { set; get; } = string.Empty;
    public string Meaning { set; get; } = string.Empty;
    public string? Structure { set; get; }
    public string? Explanation { set; get; }
    public string? Caution { set; get; }
    public bool HasExamples { set; get; }
    public IEnumerable<ExampleSentenceDTO> Examples { set; get; } = [];
}
