using Application.DTOs.Ai;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class AiGenerationService : IAiGenerationService
{
    private readonly AiGenerationSettings _settings;
    private readonly OpenAiGenerationService _openAiGenerationService;
    private readonly AnthropicGenerationService _anthropicGenerationService;
    private readonly GeminiGenerationService _geminiGenerationService;
    private readonly OpenRouterGenerationService _openRouterGenerationService;

    public AiGenerationService(
        IOptions<AiGenerationSettings> options,
        OpenAiGenerationService openAiGenerationService,
        AnthropicGenerationService anthropicGenerationService,
        GeminiGenerationService geminiGenerationService,
        OpenRouterGenerationService openRouterGenerationService)
    {
        _settings = options.Value;
        _openAiGenerationService = openAiGenerationService;
        _anthropicGenerationService = anthropicGenerationService;
        _geminiGenerationService = geminiGenerationService;
        _openRouterGenerationService = openRouterGenerationService;
    }

    public Task<string> GenerateQuestionsJsonAsync(
        JlptLevel level,
        SectionType sectionType,
        string topic,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (_settings.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            return _openAiGenerationService.GenerateQuestionsJsonAsync(level, sectionType, topic, count, cancellationToken);

        if (_settings.Provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
            return _anthropicGenerationService.GenerateQuestionsJsonAsync(level, sectionType, topic, count, cancellationToken);

        if (_settings.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            return _geminiGenerationService.GenerateQuestionsJsonAsync(level, sectionType, topic, count, cancellationToken);

        if (_settings.Provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
            return _openRouterGenerationService.GenerateQuestionsJsonAsync(level, sectionType, topic, count, cancellationToken);

        throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
    }

    public Task<AiGeneratedJsonResult> GenerateStructuredJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (_settings.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            return _openAiGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);

        if (_settings.Provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
            return _anthropicGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);

        if (_settings.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            return _geminiGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);

        if (_settings.Provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
            return _openRouterGenerationService.GenerateStructuredJsonAsync(systemPrompt, userPrompt, cancellationToken);

        throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);
    }
}
