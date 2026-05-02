using System.Text;
using Application.Settings;
using Domain.Constants;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        context.Fail(MessageConstants.CommonMessage.UNAUTHORIZED);
                        return;
                    }

                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId, context.HttpContext.RequestAborted);

                    if (user == null || !user.IsActive)
                    {
                        context.Fail(MessageConstants.CommonMessage.UNAUTHORIZED);
                        return;
                    }

                    var currentRole = user.Role.ToString().ToLowerInvariant();
                    var tokenRoles = (context.Principal?.FindAll(ClaimTypes.Role) ?? Enumerable.Empty<Claim>())
                        .Select(claim => claim.Value)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (!tokenRoles.Contains(currentRole))
                        context.Fail(MessageConstants.CommonMessage.UNAUTHORIZED);
                }
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
