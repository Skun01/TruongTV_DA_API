namespace Domain.Entities;

public class ShadowingAttempt : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string TopicId { get; set; } = string.Empty;
    public ShadowingTopic Topic { get; set; } = null!;

    public string SentenceId { get; set; } = string.Empty;
    public Sentence Sentence { get; set; } = null!;

    public string AudioAssetId { get; set; } = string.Empty;
    public MediaAsset AudioAsset { get; set; } = null!;

    public string Locale { get; set; } = "ja-JP";
    public string? RecognizedText { get; set; }
    public double? PronScore { get; set; }
    public double? AccuracyScore { get; set; }
    public double? FluencyScore { get; set; }
    public double? CompletenessScore { get; set; }
    public double? ProsodyScore { get; set; }
    public string? ErrorTypes { get; set; }
    public int? DurationMs { get; set; }
    public string? RawResultJson { get; set; }
}
