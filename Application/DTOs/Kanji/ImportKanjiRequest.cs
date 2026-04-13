using Application.DTOs.Common;
using System.Text.Json.Serialization;

namespace Application.DTOs.Kanji;

public class ImportKanjiRequest
{
    public List<ImportKanjiItemRequest> Items { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImportTemplateGuide? Guide { get; set; }
}
