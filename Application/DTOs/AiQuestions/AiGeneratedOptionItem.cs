using System.Text.Json.Serialization;

namespace Application.DTOs.AiQuestions;

public class AiGeneratedOptionItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("isCorrect")]
    public bool IsCorrect { get; set; }
}
