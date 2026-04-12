namespace Application.DTOs.Kanji;

public class ImportKanjiRequest
{
    public List<ImportKanjiItemRequest> Items { get; set; } = new();
}
