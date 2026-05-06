using System.Text.Json;
using Application.DTOs.Ai;
using Application.Helper;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace Infrastructure.InternalServices;

public class AnthropicGenerationService : IAiGenerationService
{
    private readonly AiGenerationSettings _settings;
    private readonly ILogger<AnthropicGenerationService> _logger;

    public AnthropicGenerationService(
        IOptions<AiGenerationSettings> options,
        ILogger<AnthropicGenerationService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateQuestionsJsonAsync(
        JlptLevel level,
        SectionType sectionType,
        string topic,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Anthropic.ApiKey) || string.IsNullOrWhiteSpace(_settings.Anthropic.Model))
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

        try
        {
            var client = new AnthropicClient(_settings.Anthropic.ApiKey);

            var userPrompt = AiPromptHelper.BuildPrompt(level, sectionType, topic, count);
            var systemPrompt = AiPromptHelper.GetSystemPrompt();

            var parameters = new MessageParameters
            {
                Model = _settings.Anthropic.Model,
                MaxTokens = _settings.MaxTokens,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) },
                Messages = new List<Message>
                {
                    new Message(RoleType.User, userPrompt)
                }
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            var content = response.Content
                .OfType<TextContent>()
                .Select(c => c.Text)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(content))
                throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

            var json = AiGenerationResponseParser.ExtractJson(content);
            JsonDocument.Parse(json);

            _logger.LogInformation(
                "Sinh {Count} câu hỏi JLPT {Level}/{Section} thành công, topic: {Topic}",
                count, level, sectionType, topic);

            return json;
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi Anthropic API sinh câu hỏi JLPT {Level}/{Section}", level, sectionType);
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }

    public async Task<AiGeneratedJsonResult> GenerateStructuredJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Anthropic.ApiKey) || string.IsNullOrWhiteSpace(_settings.Anthropic.Model))
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);

        try
        {
            var client = new AnthropicClient(_settings.Anthropic.ApiKey);

            var parameters = new MessageParameters
            {
                Model = _settings.Anthropic.Model,
                MaxTokens = _settings.MaxTokens,
                System = new List<SystemMessage> { new(systemPrompt) },
                Messages = new List<Message>
                {
                    new(RoleType.User, userPrompt)
                }
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            var content = response.Content
                .OfType<TextContent>()
                .Select(x => x.Text)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(content))
                throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);

            var json = AiGenerationResponseParser.ExtractJson(content, MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);
            JsonDocument.Parse(json);

            return new AiGeneratedJsonResult
            {
                Content = json,
                Model = _settings.Anthropic.Model,
            };
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi Anthropic API sinh JLPT AI analysis");
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);
        }
    }
}
