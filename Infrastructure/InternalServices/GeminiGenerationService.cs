using System.Text.Json;
using Application.Helper;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Enums;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class GeminiGenerationService : IAiGenerationService
{
    private readonly AiGenerationSettings _settings;
    private readonly ILogger<GeminiGenerationService> _logger;

    public GeminiGenerationService(
        IOptions<AiGenerationSettings> options,
        ILogger<GeminiGenerationService> logger)
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
        if (string.IsNullOrWhiteSpace(_settings.Gemini.ApiKey) || string.IsNullOrWhiteSpace(_settings.Gemini.Model))
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);

        try
        {
            var client = new Client(apiKey: _settings.Gemini.ApiKey);

            var userPrompt = AiPromptHelper.BuildPrompt(level, sectionType, topic, count);
            var systemPrompt = AiPromptHelper.GetSystemPrompt();

            var response = await client.Models.GenerateContentAsync(
                model: _settings.Gemini.Model,
                contents: userPrompt,
                config: new GenerateContentConfig
                {
                    SystemInstruction = new Content
                    {
                        Parts = new List<Part>
                        {
                            new() { Text = systemPrompt }
                        }
                    },
                    MaxOutputTokens = _settings.MaxTokens,
                    ResponseMimeType = "application/json"
                },
                cancellationToken: cancellationToken);

            var content = string.Concat(
                response.Candidates?
                    .SelectMany(candidate => candidate.Content?.Parts ?? Enumerable.Empty<Part>())
                    .Select(part => part.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                ?? Enumerable.Empty<string>());

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
            _logger.LogError(ex, "Lỗi gọi Gemini API sinh câu hỏi JLPT {Level}/{Section}", level, sectionType);
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }
}
