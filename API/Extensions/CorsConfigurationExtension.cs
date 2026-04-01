namespace API.Extensions;

public static class CorsConfigurationExtension
{
    public const string FrontendPolicyName = "Frontend";

    public static IServiceCollection AddCorsConfigurationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://localhost:5174"];

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
