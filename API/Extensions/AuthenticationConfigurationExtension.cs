using System.Text;
using Application.Settings;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class AuthenticationConfigurationExtension
{
    public static IServiceCollection AddAuthConfigurationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role,
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicyConstants.AdminOnly, policy =>
                policy.RequireRole("admin"));

            options.AddPolicy(AuthPolicyConstants.EditorOrAdmin, policy =>
                policy.RequireRole("editor", "admin"));
        });

        // Lấy từ Settings
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .PostConfigure<IOptions<JwtSettings>>((options, jwtSettingsAccessor) =>
        {
            var jwtSettings = jwtSettingsAccessor.Value;
            options.TokenValidationParameters.ValidIssuer = jwtSettings.Issuer;
            options.TokenValidationParameters.ValidAudience = jwtSettings.Audience;
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key!));
        });

        return services;    
    }
}
