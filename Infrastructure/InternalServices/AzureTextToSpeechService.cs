using System.Net.Http.Headers;
using System.Text;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

/// <summary>
/// Azure Cognitive Services Text-to-Speech — dùng REST API
/// Endpoint: https://{region}.tts.speech.microsoft.com/cognitiveservices/v1
/// </summary>
public class AzureTextToSpeechService : ITextToSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly AzureSpeechSettings _settings;
    private readonly ILogger<AzureTextToSpeechService> _logger;

    public AzureTextToSpeechService(
        HttpClient httpClient,
        IOptions<AzureSpeechSettings> options,
        ILogger<AzureTextToSpeechService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Stream> SynthesizeAsync(
        string text,
        string voiceName = "ja-JP-NanamiNeural",
        CancellationToken cancellationToken = default)
    {
        var subscriptionKey = _settings.SubscriptionKey?.Trim();
        var region = _settings.Region?.Trim();

        if (string.IsNullOrWhiteSpace(subscriptionKey) || string.IsNullOrWhiteSpace(region))
            throw new ApplicationException(MessageConstants.ShadowingMessage.AZURE_NOT_CONFIGURED);

        var endpoint = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

        // Tạo SSML payload
        var ssml = BuildSsml(text, voiceName);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-16khz-128kbitrate-mono-mp3");
        request.Headers.Add("User-Agent", "TachoLearning");
        request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        _logger.LogInformation(
            "Gửi yêu cầu TTS tới Azure Speech. Voice: {Voice}. Text length: {Length}",
            voiceName, text.Length);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Azure TTS thất bại. Status: {StatusCode}. Response: {Response}",
                (int)response.StatusCode, errorBody);
            throw new ApplicationException(MessageConstants.AiQuestionMessage.GENERATION_FAILED);
        }

        _logger.LogInformation("Azure TTS thành công. Voice: {Voice}", voiceName);

        // Trả về audio stream (MP3)
        var audioStream = new MemoryStream();
        await response.Content.CopyToAsync(audioStream, cancellationToken);
        audioStream.Position = 0;
        return audioStream;
    }

    /// <summary>
    /// Tạo SSML (Speech Synthesis Markup Language) cho Azure TTS
    /// </summary>
    private static string BuildSsml(string text, string voiceName)
    {
        // Escape XML characters trong text
        var escapedText = System.Security.SecurityElement.Escape(text);

        return $"""
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='ja-JP'>
                <voice name='{voiceName}'>
                    <prosody rate='0.9'>{escapedText}</prosody>
                </voice>
            </speak>
            """;
    }
}
