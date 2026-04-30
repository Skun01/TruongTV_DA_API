namespace Application.Settings;

public class AiGenerationSettings
{
    public const string SectionName = "AiGeneration";

    public string Provider { get; set; } = "Anthropic";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public int MaxTokens { get; set; } = 4096;
}
