using System.Text.Json.Serialization;

namespace Application.DTOs.AiQuestions;

/// <summary>
/// Cấu trúc JSON mà AI trả về — dùng để parse GeneratedData
/// </summary>
public class AiGeneratedQuestionData
{
    [JsonPropertyName("passage")]
    public string? Passage { get; set; }

    [JsonPropertyName("script")]
    public string? Script { get; set; }

    [JsonPropertyName("questions")]
    public List<AiGeneratedQuestionItem> Questions { get; set; } = new();
}
