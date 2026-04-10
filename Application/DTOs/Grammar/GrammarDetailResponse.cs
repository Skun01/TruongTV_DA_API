using Application.DTOs.CardNotes;

namespace Application.DTOs.Grammar;

public class GrammarDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<GrammarStructureResponse> Structures { get; set; } = new();
    public string? Explanation { get; set; }
    public string? Caution { get; set; }
    public string? Register { get; set; }
    public List<string> AlternateForms { get; set; } = new();
    public List<GrammarRelationResponse> Relations { get; set; } = new();
    public List<GrammarResourceResponse> Resources { get; set; } = new();
    public List<GrammarSentenceResponse> Sentences { get; set; } = new();
    public List<CardNoteResponse> UserNotes { get; set; } = new();
}
