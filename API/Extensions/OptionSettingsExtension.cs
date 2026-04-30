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
            .Bind(configuration.GetSection(AiGenerationSettings.SectionName));

        return services;
    }
}
