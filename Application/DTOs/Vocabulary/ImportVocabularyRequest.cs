using Application.DTOs.Common;
using System.Text.Json.Serialization;

namespace Application.DTOs.Vocabulary;

public class ImportVocabularyRequest
{
    public List<ImportVocabularyItemRequest> Items { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImportTemplateGuide? Guide { get; set; }
}
