using System.Text.Json.Serialization;

namespace Application.DTOs.AiQuestions;

public class AiGeneratedQuestionItem
{
    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("options")]
    public List<AiGeneratedOptionItem> Options { get; set; } = new();
}
