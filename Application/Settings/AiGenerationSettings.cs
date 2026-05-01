namespace Application.Settings;

public class AiGenerationSettings
{
    public const string SectionName = "AiGeneration";

    public string Provider { get; set; } = "OpenAI";
    public int MaxTokens { get; set; } = 4096;
    public AiProviderSettings OpenAI { get; set; } = new();
    public AiProviderSettings Anthropic { get; set; } = new();
    public AiProviderSettings Gemini { get; set; } = new();
    public AiProviderSettings OpenRouter { get; set; } = new();
}

public class AiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
