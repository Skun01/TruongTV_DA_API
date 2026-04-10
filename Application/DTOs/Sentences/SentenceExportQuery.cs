namespace Application.DTOs.Sentences;

public class SentenceExportQuery
{
    public string? Q { get; set; }
    public string? Level { get; set; }
    public bool CreatedByMe { get; set; } = false;
    public bool? HasAudio { get; set; }
}
