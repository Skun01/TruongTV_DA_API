using Application.DTOs.Common;
using System.Text.Json.Serialization;

namespace Application.DTOs.Sentences;

public class ImportSentenceRequest
{
    public List<ImportSentenceItemRequest> Items { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImportTemplateGuide? Guide { get; set; }
}
