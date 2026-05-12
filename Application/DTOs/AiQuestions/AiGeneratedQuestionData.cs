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

    [JsonPropertyName("difficulty")]
    public AiGeneratedQuestionDifficultyInfo? Difficulty { get; set; }

    [JsonPropertyName("metadata")]
    public AiGeneratedQuestionMetadata? Metadata { get; set; }

    [JsonPropertyName("questions")]
    public List<AiGeneratedQuestionItem> Questions { get; set; } = new();
}

public class AiGeneratedQuestionDifficultyInfo
{
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class AiGeneratedQuestionMetadata
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("qualityScore")]
    public int QualityScore { get; set; }

    [JsonPropertyName("requiresManualReview")]
    public bool RequiresManualReview { get; set; }

    [JsonPropertyName("validationWarnings")]
    public List<string> ValidationWarnings { get; set; } = new();

    [JsonPropertyName("duplicateCandidates")]
    public List<AiGeneratedQuestionDuplicateCandidate> DuplicateCandidates { get; set; } = new();
}

public class AiGeneratedQuestionDuplicateCandidate
{
    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; } = string.Empty;

    [JsonPropertyName("sourceId")]
    public string SourceId { get; set; } = string.Empty;

    [JsonPropertyName("previewText")]
    public string PreviewText { get; set; } = string.Empty;

    [JsonPropertyName("similarityScore")]
    public double SimilarityScore { get; set; }
}
