using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Services;
using Infrastructure.InternalServices;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services;
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDeckService, DeckService>();
        services.AddScoped<IVocabularyCardService, VocabularyCardService>();
        services.AddScoped<IGrammarCardService, GrammarCardService>();

        // Internal services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        
        return services;
    }
}