using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTOs.Ai;
using Application.Helper;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class OpenRouterGenerationService : IAiGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly AiGenerationSettings _settings;
    private readonly ILogger<OpenRouterGenerationService> _logger;

    public OpenRouterGenerationService(
        HttpClient httpClient,
        IOptions<AiGenerationSettings> options,
        ILogger<OpenRouterGenerationService> logger)
    {
        _httpClient = httpClient;
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
        if (string.IsNullOrWhiteSpace(_settings.OpenRouter.ApiKey)
            || string.IsNullOrWhiteSpace(_settings.OpenRouter.Model)
            || string.IsNullOrWhiteSpace(_settings.OpenRouter.BaseUrl))
        {
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }

        try
        {
            var userPrompt = AiPromptHelper.BuildPrompt(level, sectionType, topic, count);
            var systemPrompt = AiPromptHelper.GetSystemPrompt();

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_settings.OpenRouter.BaseUrl), "chat/completions"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenRouter.ApiKey);

            var payload = new OpenRouterChatRequest
            {
                Model = _settings.OpenRouter.Model,
                MaxTokens = _settings.MaxTokens,
                Messages =
                [
                    new OpenRouterChatMessage { Role = "system", Content = systemPrompt },
                    new OpenRouterChatMessage { Role = "user", Content = userPrompt }
                ]
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<OpenRouterChatResponse>(responseContent);
            var content = data?.Choices?.FirstOrDefault()?.Message?.Content;

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
            _logger.LogError(ex, "Lỗi gọi OpenRouter API sinh câu hỏi JLPT {Level}/{Section}", level, sectionType);
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }
    }

    public async Task<AiGeneratedJsonResult> GenerateStructuredJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenRouter.ApiKey)
            || string.IsNullOrWhiteSpace(_settings.OpenRouter.Model)
            || string.IsNullOrWhiteSpace(_settings.OpenRouter.BaseUrl))
        {
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_settings.OpenRouter.BaseUrl), "chat/completions"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenRouter.ApiKey);

            var payload = new OpenRouterChatRequest
            {
                Model = _settings.OpenRouter.Model,
                MaxTokens = _settings.MaxTokens,
                Messages =
                [
                    new OpenRouterChatMessage { Role = "system", Content = systemPrompt },
                    new OpenRouterChatMessage { Role = "user", Content = userPrompt }
                ]
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<OpenRouterChatResponse>(responseContent);
            var content = data?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
                throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);

            var json = AiGenerationResponseParser.ExtractJson(content, MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);
            JsonDocument.Parse(json);

            return new AiGeneratedJsonResult
            {
                Content = json,
                Model = _settings.OpenRouter.Model,
            };
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi OpenRouter API sinh JLPT AI analysis");
            throw new ApplicationException(MessageConstants.ExamSessionMessage.AI_ANALYSIS_UNAVAILABLE);
        }
    }

    private class OpenRouterChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("messages")]
        public List<OpenRouterChatMessage> Messages { get; set; } = [];
    }

    private class OpenRouterChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class OpenRouterChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenRouterChoice>? Choices { get; set; }
    }

    private class OpenRouterChoice
    {
        [JsonPropertyName("message")]
        public OpenRouterChatMessage? Message { get; set; }
    }
}
