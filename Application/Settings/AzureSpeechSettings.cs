namespace Application.Settings;

public class AzureSpeechSettings
{
    public const string SectionName = "AzureSpeechConfig";

    public string Endpoint { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string SubscriptionKey { get; set; } = string.Empty;
}
