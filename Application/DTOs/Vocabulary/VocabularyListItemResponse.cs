namespace Application.DTOs.Vocabulary;

public class VocabularyListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string Writing { get; set; } = string.Empty;
    public string? Reading { get; set; }
    public string? WordType { get; set; }
}
