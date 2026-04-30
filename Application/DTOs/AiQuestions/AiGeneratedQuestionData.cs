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

public class AiGeneratedQuestionItem
{
    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("options")]
    public List<AiGeneratedOptionItem> Options { get; set; } = new();
}

public class AiGeneratedOptionItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("isCorrect")]
    public bool IsCorrect { get; set; }
}
