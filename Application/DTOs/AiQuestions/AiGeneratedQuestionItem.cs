using System.Text.Json.Serialization;

namespace Application.DTOs.AiQuestions;

public class AiGeneratedQuestionItem
{
    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("difficultyScore")]
    public int? DifficultyScore { get; set; }

    [JsonPropertyName("skillTags")]
    public List<string> SkillTags { get; set; } = new();

    [JsonPropertyName("options")]
    public List<AiGeneratedOptionItem> Options { get; set; } = new();
}
