using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class GrammarDetail
{
    public string CardId { get; set; } = string.Empty;
    public Card Card { get; set; } = null!;

    public List<GrammarStructureItem> Structures { get; set; } = new();
    public string? Explanation { get; set; }
    public string? Caution { get; set; }
    public RegisterType? Register { get; set; }
    public List<string> AlternateForms { get; set; } = new();
}
