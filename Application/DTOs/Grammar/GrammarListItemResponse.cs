namespace Application.DTOs.Grammar;

public class GrammarListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? Register { get; set; }
    public int StructuresCount { get; set; }
    public List<string> AlternateForms { get; set; } = new();
}
