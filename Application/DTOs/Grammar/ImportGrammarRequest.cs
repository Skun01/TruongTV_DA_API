using Application.DTOs.Common;
using System.Text.Json.Serialization;

namespace Application.DTOs.Grammar;

public class ImportGrammarRequest
{
    public List<ImportGrammarItemRequest> Items { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImportTemplateGuide? Guide { get; set; }
}
