using System.Text.Json;
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
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

        try
        {
            var client = new AnthropicClient(_settings.ApiKey);

            var userPrompt = AiPromptHelper.BuildPrompt(level, sectionType, topic, count);
            var systemPrompt = AiPromptHelper.GetSystemPrompt();

            var parameters = new MessageParameters
            {
                Model = _settings.Model,
                MaxTokens = _settings.MaxTokens,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) },
                Messages = new List<Message>
                {
                    new Message(RoleType.User, userPrompt)
                }
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            // Lấy text content từ response
            var content = response.Content
                .OfType<TextContent>()
                .Select(c => c.Text)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(content))
                throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

            // Trích xuất JSON từ response (AI có thể wrap trong ```json ... ```)
            var json = ExtractJson(content);

            // Validate JSON hợp lệ
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

    /// <summary>
    /// Trích xuất JSON từ response — xử lý trường hợp AI wrap trong markdown code block
    /// </summary>
    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();

        // Trường hợp AI wrap trong ```json ... ```
        if (trimmed.StartsWith("```"))
        {
            var startIdx = trimmed.IndexOf('\n');
            if (startIdx == -1) return trimmed;

            var endIdx = trimmed.LastIndexOf("```");
            if (endIdx <= startIdx) return trimmed;

            return trimmed.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();
        }

        // Trường hợp JSON thuần
        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            return trimmed;

        // Tìm vị trí JSON bắt đầu
        var jsonStart = trimmed.IndexOf('{');
        if (jsonStart == -1)
            jsonStart = trimmed.IndexOf('[');

        if (jsonStart == -1)
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

        return trimmed.Substring(jsonStart);
    }
}
