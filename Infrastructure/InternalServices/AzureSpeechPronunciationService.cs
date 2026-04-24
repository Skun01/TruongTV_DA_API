using System.Text;
using System.Text.Json;
using Application.DTOs.Internal;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class AzureSpeechPronunciationService : IPronunciationAssessmentService
{
    private readonly HttpClient _httpClient;
    private readonly AzureSpeechSettings _settings;

    public AzureSpeechPronunciationService(HttpClient httpClient, IOptions<AzureSpeechSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<PronunciationAssessmentResult> AssessAsync(
        Stream audioStream,
        string contentType,
        string referenceText,
        string locale,
        CancellationToken cancellationToken = default)
    {
        var subscriptionKey = _settings.SubscriptionKey?.Trim();
        var baseEndpoint = ResolveBaseEndpoint(_settings);
        if (string.IsNullOrWhiteSpace(baseEndpoint) || string.IsNullOrWhiteSpace(subscriptionKey))
            throw new ApplicationException(MessageConstants.ShadowingMessage.AZURE_NOT_CONFIGURED);

        var normalizedLocale = string.IsNullOrWhiteSpace(locale) ? "ja-JP" : locale.Trim();
        var normalizedReferenceText = referenceText?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedReferenceText))
            throw new ApplicationException(MessageConstants.ShadowingMessage.SENTENCE_NOT_FOUND);

        var pronunciationConfig = new
        {
            ReferenceText = normalizedReferenceText,
            GradingSystem = "HundredMark",
            Granularity = "Word",
            Dimension = "Comprehensive",
            EnableMiscue = true,
        };

        var pronunciationHeaderValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pronunciationConfig)));

        var endpoint = $"{baseEndpoint.TrimEnd('/')}/speech/recognition/conversation/cognitiveservices/v1" +
            $"?language={Uri.EscapeDataString(normalizedLocale)}&format=detailed";

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        request.Headers.Add("Pronunciation-Assessment", pronunciationHeaderValue);
        request.Content = new StreamContent(audioStream);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException(MessageConstants.ShadowingMessage.ASSESSMENT_FAILED);

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(rawJson);

        var result = new PronunciationAssessmentResult
        {
            RawJson = rawJson,
        };

        if (document.RootElement.TryGetProperty("DisplayText", out var displayText))
            result.RecognizedText = displayText.GetString();

        if (document.RootElement.TryGetProperty("Duration", out var durationElement)
            && durationElement.ValueKind == JsonValueKind.Number
            && durationElement.TryGetInt64(out var durationTicks))
        {
            result.DurationMs = (int)(durationTicks / 10000);
        }

        if (document.RootElement.TryGetProperty("NBest", out var nBestElement)
            && nBestElement.ValueKind == JsonValueKind.Array
            && nBestElement.GetArrayLength() > 0)
        {
            var firstBest = nBestElement[0];
            if (firstBest.TryGetProperty("PronunciationAssessment", out var assessment))
            {
                result.PronScore = TryGetDouble(assessment, "PronScore");
                result.AccuracyScore = TryGetDouble(assessment, "AccuracyScore");
                result.FluencyScore = TryGetDouble(assessment, "FluencyScore");
                result.CompletenessScore = TryGetDouble(assessment, "CompletenessScore");
                result.ProsodyScore = TryGetDouble(assessment, "ProsodyScore");
            }

            if (firstBest.TryGetProperty("Words", out var wordsElement) && wordsElement.ValueKind == JsonValueKind.Array)
            {
                var errorTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var word in wordsElement.EnumerateArray())
                {
                    if (!word.TryGetProperty("PronunciationAssessment", out var wordAssessment))
                        continue;

                    if (!wordAssessment.TryGetProperty("ErrorType", out var errorTypeElement))
                        continue;

                    var errorType = errorTypeElement.GetString();
                    if (!string.IsNullOrWhiteSpace(errorType) && !string.Equals(errorType, "None", StringComparison.OrdinalIgnoreCase))
                        errorTypes.Add(errorType);
                }

                result.ErrorTypes = errorTypes.ToList();
            }
        }

        return result;
    }

    private static double? TryGetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return null;

        if (value.ValueKind != JsonValueKind.Number)
            return null;

        return value.TryGetDouble(out var score) ? score : null;
    }

    private static string? ResolveBaseEndpoint(AzureSpeechSettings settings)
    {
        var endpoint = settings.Endpoint?.Trim();
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out _))
                return endpoint.TrimEnd('/');

            if (!endpoint.Contains("://", StringComparison.Ordinal))
                return $"https://{endpoint}.stt.speech.microsoft.com";
        }

        var region = settings.Region?.Trim();
        if (!string.IsNullOrWhiteSpace(region))
            return $"https://{region}.stt.speech.microsoft.com";

        return null;
    }
}
