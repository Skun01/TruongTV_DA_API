using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.DTOs.Internal;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class AzureSpeechPronunciationService : IPronunciationAssessmentService
{
    private readonly HttpClient _httpClient;
    private readonly AzureSpeechSettings _settings;
    private readonly ILogger<AzureSpeechPronunciationService> _logger;

    public AzureSpeechPronunciationService(
        HttpClient httpClient,
        IOptions<AzureSpeechSettings> options,
        ILogger<AzureSpeechPronunciationService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
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

        if (audioStream.CanSeek)
            audioStream.Position = 0;

        if (!MediaTypeHeaderValue.TryParse(contentType, out var mediaTypeHeaderValue))
            throw new ApplicationException(MessageConstants.ShadowingMessage.INVALID_AUDIO);

        using var preparedStream = new MemoryStream();
        await audioStream.CopyToAsync(preparedStream, cancellationToken);
        preparedStream.Position = 0;

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        request.Headers.Add("Pronunciation-Assessment", pronunciationHeaderValue);
        request.Headers.TransferEncodingChunked = false;
        request.Content = new StreamContent(preparedStream);
        request.Content.Headers.ContentType = mediaTypeHeaderValue;
        request.Content.Headers.ContentLength = preparedStream.Length;

        _logger.LogInformation(
            "Sending audio to Azure Speech. Content-Type: {ContentType}. Length: {Length}",
            mediaTypeHeaderValue.ToString(),
            preparedStream.Length);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Azure Speech pronunciation assessment failed. Status: {StatusCode}. Content-Type: {ContentType}. Endpoint: {Endpoint}. Response: {Response}",
                (int)response.StatusCode,
                mediaTypeHeaderValue.ToString(),
                endpoint,
                errorBody);
            throw new ApplicationException(MessageConstants.ShadowingMessage.ASSESSMENT_FAILED);
        }

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation(
            "Azure Speech pronunciation assessment succeeded. Content-Type: {ContentType}. Endpoint: {Endpoint}",
            mediaTypeHeaderValue.ToString(),
            endpoint);
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
            _logger.LogInformation(
                "Azure Speech first NBest payload: {Payload}",
                firstBest.GetRawText());

            if (firstBest.TryGetProperty("PronunciationAssessment", out var assessment))
            {
                result.PronScore = TryGetDouble(assessment, "PronScore");
                result.AccuracyScore = TryGetDouble(assessment, "AccuracyScore");
                result.FluencyScore = TryGetDouble(assessment, "FluencyScore");
                result.CompletenessScore = TryGetDouble(assessment, "CompletenessScore");
                result.ProsodyScore = TryGetDouble(assessment, "ProsodyScore");
            }

            result.PronScore ??= TryGetDouble(firstBest, "PronScore");
            result.AccuracyScore ??= TryGetDouble(firstBest, "AccuracyScore");
            result.FluencyScore ??= TryGetDouble(firstBest, "FluencyScore");
            result.CompletenessScore ??= TryGetDouble(firstBest, "CompletenessScore");
            result.ProsodyScore ??= TryGetDouble(firstBest, "ProsodyScore");

            if (firstBest.TryGetProperty("Words", out var wordsElement) && wordsElement.ValueKind == JsonValueKind.Array)
            {
                var errorTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var wordResults = new List<PronunciationAssessmentWordResult>();

                foreach (var word in wordsElement.EnumerateArray())
                {
                    var token = TryGetString(word, "Word") ?? string.Empty;
                    var displayWord = TryGetString(word, "DisplayWord") ?? token;
                    double? accuracyScore = null;
                    var wordErrorType = TryGetString(word, "ErrorType");

                    if (word.TryGetProperty("PronunciationAssessment", out var wordAssessment))
                    {
                        accuracyScore = TryGetDouble(wordAssessment, "AccuracyScore");

                        if (wordAssessment.TryGetProperty("ErrorType", out var nestedErrorTypeElement))
                        {
                            var nestedErrorType = nestedErrorTypeElement.GetString();
                            wordErrorType ??= nestedErrorType;

                            if (!string.IsNullOrWhiteSpace(nestedErrorType) && !string.Equals(nestedErrorType, "None", StringComparison.OrdinalIgnoreCase))
                                errorTypes.Add(nestedErrorType);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(wordErrorType) && word.TryGetProperty("ErrorType", out var errorTypeElement))
                    {
                        var errorType = errorTypeElement.GetString();
                        wordErrorType = errorType;

                        if (!string.IsNullOrWhiteSpace(errorType) && !string.Equals(errorType, "None", StringComparison.OrdinalIgnoreCase))
                            errorTypes.Add(errorType);
                    }

                    accuracyScore ??= TryGetDouble(word, "AccuracyScore");

                    if (string.IsNullOrWhiteSpace(token) && string.IsNullOrWhiteSpace(displayWord))
                        continue;

                    wordResults.Add(new PronunciationAssessmentWordResult
                    {
                        Word = token,
                        DisplayWord = displayWord,
                        AccuracyScore = accuracyScore,
                        ErrorType = wordErrorType,
                    });
                }

                result.ErrorTypes = errorTypes.ToList();
                result.Words = wordResults;
            }
        }

        if (!string.IsNullOrWhiteSpace(result.RecognizedText)
            && string.Equals(result.RecognizedText.Trim(), ".", StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Azure Speech returned punctuation-only recognized text. Locale: {Locale}. ReferenceText: {ReferenceText}. RawJson: {RawJson}",
                normalizedLocale,
                normalizedReferenceText,
                rawJson);
        }

        return result;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
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
            if (!endpoint.Contains("://", StringComparison.Ordinal))
                endpoint = $"https://{endpoint}";

            if (Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
                return ResolveSpeechHost(endpointUri);
        }

        var region = settings.Region?.Trim();
        if (!string.IsNullOrWhiteSpace(region))
            return $"https://{region}.stt.speech.microsoft.com";

        return null;
    }

    private static string ResolveSpeechHost(Uri endpointUri)
    {
        if (endpointUri.Host.EndsWith(".api.cognitive.microsoft.com", StringComparison.OrdinalIgnoreCase))
        {
            var region = endpointUri.Host[..endpointUri.Host.IndexOf(".api.cognitive.microsoft.com", StringComparison.OrdinalIgnoreCase)];
            return $"{endpointUri.Scheme}://{region}.stt.speech.microsoft.com";
        }

        if (endpointUri.Host.EndsWith(".cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase))
            return $"{endpointUri.Scheme}://{endpointUri.Host}";

        return endpointUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }
}
