using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class OpenRouterConversationService : IAiConversationService
{
    private readonly HttpClient _httpClient;
    private readonly AiGenerationSettings _settings;
    private readonly ILogger<OpenRouterConversationService> _logger;

    public OpenRouterConversationService(
        HttpClient httpClient,
        IOptions<AiGenerationSettings> options,
        ILogger<OpenRouterConversationService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateConversationJsonAsync(
        string systemPrompt,
        string userPrompt,
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

            _logger.LogInformation("AI Conversation generation thành công");

            return json;
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi OpenRouter API cho conversation");
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
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
