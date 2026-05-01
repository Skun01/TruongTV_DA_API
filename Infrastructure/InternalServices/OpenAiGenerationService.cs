using System.Text.Json;
using Application.Helper;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Infrastructure.InternalServices;

public class OpenAiGenerationService : IAiGenerationService
{
    private readonly AiGenerationSettings _settings;
    private readonly ILogger<OpenAiGenerationService> _logger;

    public OpenAiGenerationService(
        IOptions<AiGenerationSettings> options,
        ILogger<OpenAiGenerationService> logger)
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
        if (string.IsNullOrWhiteSpace(_settings.OpenAI.ApiKey) || string.IsNullOrWhiteSpace(_settings.OpenAI.Model))
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

        try
        {
            var client = new ChatClient(_settings.OpenAI.Model, _settings.OpenAI.ApiKey);

            var userPrompt = AiPromptHelper.BuildPrompt(level, sectionType, topic, count);
            var systemPrompt = AiPromptHelper.GetSystemPrompt();

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _settings.MaxTokens
            };

            var response = await client.CompleteChatAsync(messages, options, cancellationToken);

            var content = string.Concat(response.Value.Content.Select(c => c.Text));

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
            _logger.LogError(ex, "Lỗi gọi OpenAI API sinh câu hỏi JLPT {Level}/{Section}", level, sectionType);
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }

}
