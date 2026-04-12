namespace Application.DTOs.Kanji;

public class KanjiRadicalResponse
{
    public string Id { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public string MeaningVi { get; set; } = string.Empty;
    public string? KanjiCardId { get; set; }
}
