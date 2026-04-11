using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.DTOs.Grammar;

public class GrammarRelationResponse
{
    public string RelatedId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GrammarRelationType RelationType { get; set; }
}
