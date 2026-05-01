using Application.Settings;

namespace API.Extensions;

public static class OptionSettingsExtension
{
    public static IServiceCollection AddOptionSettingsExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AppSettings>()
            .Bind(configuration.GetSection(AppSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CloudinarySettings>()
            .Bind(configuration.GetSection(CloudinarySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<VoicevoxSettings>()
            .Bind(configuration.GetSection(VoicevoxSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AzureSpeechSettings>()
            .Bind(configuration.GetSection(AzureSpeechSettings.SectionName));

        services.AddOptions<AiGenerationSettings>()
            .Bind(configuration.GetSection(AiGenerationSettings.SectionName))
            .Validate(settings =>
            {
                if (string.IsNullOrWhiteSpace(settings.Provider))
                    return false;

                var provider = settings.Provider.Trim();

                if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrWhiteSpace(settings.OpenAI.ApiKey)
                        && !string.IsNullOrWhiteSpace(settings.OpenAI.Model);

                if (provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrWhiteSpace(settings.Anthropic.ApiKey)
                        && !string.IsNullOrWhiteSpace(settings.Anthropic.Model);

                if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrWhiteSpace(settings.Gemini.ApiKey)
                        && !string.IsNullOrWhiteSpace(settings.Gemini.Model);

                if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrWhiteSpace(settings.OpenRouter.ApiKey)
                        && !string.IsNullOrWhiteSpace(settings.OpenRouter.Model)
                        && !string.IsNullOrWhiteSpace(settings.OpenRouter.BaseUrl);

                return false;
            }, "AiGeneration configuration is invalid.")
            .ValidateOnStart();

        return services;
    }
}
