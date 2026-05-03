using Application.IRepositories;
using Application.IServices;
using Application.IServices.IInternal;
using Application.Services;
using Infrastructure.BackgroundJobs;
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
            options.UseNpgsql(connectionString);
        });

        // Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services;
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVocabularyDetailService, VocabularyDetailService>();
        services.AddScoped<IGrammarService, GrammarService>();
        services.AddScoped<IKanjiService, KanjiService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IDeckTypeService, DeckTypeService>();
        services.AddScoped<IDeckTypeAdminService, DeckTypeAdminService>();
        services.AddScoped<IDeckUserService, DeckUserService>();
        services.AddScoped<IDeckAdminService, DeckAdminService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IAdminLearningService, AdminLearningService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<ILearningService, LearningService>();
        services.AddScoped<IUserLearningSettingsService, UserLearningSettingsService>();
        services.AddScoped<ICardNoteService, CardNoteService>();
        services.AddScoped<IShadowingService, ShadowingService>();
        services.AddScoped<IAdminShadowingService, AdminShadowingService>();
        services.AddScoped<ISentenceService, SentenceService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddHttpClient<IPronunciationAssessmentService, AzureSpeechPronunciationService>();

        // JLPT Exam services
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IExamSessionService, ExamSessionService>();
        services.AddScoped<IAiQuestionService, AiQuestionService>();

        // AI Generation
        services.AddScoped<OpenAiGenerationService>();
        services.AddScoped<AnthropicGenerationService>();
        services.AddScoped<GeminiGenerationService>();
        services.AddHttpClient<OpenRouterGenerationService>();
        services.AddScoped<IAiGenerationService, AiGenerationService>();

        // TTS — Azure Cognitive Services Text-to-Speech
        services.AddHttpClient<ITextToSpeechService, AzureTextToSpeechService>();

        // Background Job — Session Timeout
        services.AddHostedService<ExamSessionTimeoutService>();

        return services;
    }
}
