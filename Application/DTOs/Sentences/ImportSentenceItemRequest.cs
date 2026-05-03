namespace Application.DTOs.Sentences;

public class ImportSentenceItemRequest
{
    public int? RowNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Level { get; set; }
}