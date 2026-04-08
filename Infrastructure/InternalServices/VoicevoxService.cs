using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.DTOs.Internal;
using Application.DTOs.Voicevox;
using Application.IServices.IInternal;
using Application.Settings;
using Domain.Constants;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class VoicevoxService : IVoicevoxService
{
    private readonly HttpClient _httpClient;
    private readonly VoicevoxSettings _settings;

    public VoicevoxService(HttpClient httpClient, IOptions<VoicevoxSettings> options)
    {
        _settings = options.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<VoicevoxSynthesisResult?> SynthesizeAsync(string text, int? speakerId = null, CancellationToken cancellationToken = default)
    {
        var normalizedText = text?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedText))
            return null;

        var selectedSpeaker = speakerId ?? _settings.Speaker;
        if (!VoicevoxConstants.RecommendedSpeakerIdSet.Contains(selectedSpeaker))
            throw new ApplicationException(MessageConstants.CommonMessage.INVALID);

        var fileName = BuildCacheFileName(normalizedText, selectedSpeaker);
        var fullStoragePath = ResolveStoragePath(_settings.StoragePath);
        Directory.CreateDirectory(fullStoragePath);

        var audioQueryJson = await GetAudioQueryJsonAsync(normalizedText, selectedSpeaker, cancellationToken);
        var pitchPattern = ExtractPitchPattern(audioQueryJson);

        var fullFilePath = Path.Combine(fullStoragePath, fileName);
        if (File.Exists(fullFilePath))
        {
            return new VoicevoxSynthesisResult
            {
                AudioUrl = BuildPublicAudioUrl(_settings.StoragePath, fileName),
                SpeakerId = selectedSpeaker,
                PitchPattern = pitchPattern,
            };
        }

        var audioBytes = await SynthesizeAudioBytesAsync(audioQueryJson, selectedSpeaker, cancellationToken);
        await File.WriteAllBytesAsync(fullFilePath, audioBytes, cancellationToken);

        return new VoicevoxSynthesisResult
        {
            AudioUrl = BuildPublicAudioUrl(_settings.StoragePath, fileName),
            SpeakerId = selectedSpeaker,
            PitchPattern = pitchPattern,
        };
    }

    public async Task<List<VoicevoxSpeakerResponse>> GetSpeakersAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "speakers");
        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);

        var results = new List<VoicevoxSpeakerResponse>();

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return results;

        foreach (var character in document.RootElement.EnumerateArray())
        {
            var characterName = character.TryGetProperty("name", out var nameProperty)
                ? nameProperty.GetString() ?? string.Empty
                : string.Empty;

            if (!character.TryGetProperty("styles", out var stylesProperty)
                || stylesProperty.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var style in stylesProperty.EnumerateArray())
            {
                if (!style.TryGetProperty("id", out var idProperty)
                    || idProperty.ValueKind != JsonValueKind.Number
                    || !idProperty.TryGetInt32(out var styleId))
                {
                    continue;
                }

                if (!VoicevoxConstants.RecommendedSpeakerIdSet.Contains(styleId))
                    continue;

                var styleName = style.TryGetProperty("name", out var styleNameProperty)
                    ? styleNameProperty.GetString() ?? string.Empty
                    : string.Empty;

                results.Add(new VoicevoxSpeakerResponse
                {
                    SpeakerId = styleId,
                    CharacterName = characterName,
                    StyleName = styleName,
                });
            }
        }

        return results;
    }

    private async Task<string> GetAudioQueryJsonAsync(string text, int speakerId, CancellationToken cancellationToken)
    {
        using var audioQueryRequest = new HttpRequestMessage(HttpMethod.Post,
            $"audio_query?text={Uri.EscapeDataString(text)}&speaker={speakerId}");
        var audioQueryResponse = await _httpClient.SendAsync(audioQueryRequest, cancellationToken);
        if (!audioQueryResponse.IsSuccessStatusCode)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return await audioQueryResponse.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<byte[]> SynthesizeAudioBytesAsync(string audioQueryJson, int speakerId, CancellationToken cancellationToken)
    {
        using var synthesisRequest = new HttpRequestMessage(HttpMethod.Post, $"synthesis?speaker={speakerId}")
        {
            Content = new StringContent(audioQueryJson, Encoding.UTF8, "application/json")
        };

        var synthesisResponse = await _httpClient.SendAsync(synthesisRequest, cancellationToken);
        if (!synthesisResponse.IsSuccessStatusCode)
            throw new ApplicationException(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR);

        return await synthesisResponse.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private static List<int>? ExtractPitchPattern(string audioQueryJson)
    {
        using var document = JsonDocument.Parse(audioQueryJson);
        if (!document.RootElement.TryGetProperty("accent_phrases", out var accentPhrases)
            || accentPhrases.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var pitchPattern = new List<int>();

        foreach (var phrase in accentPhrases.EnumerateArray())
        {
            if (!phrase.TryGetProperty("moras", out var moras)
                || moras.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var mora in moras.EnumerateArray())
            {
                if (!mora.TryGetProperty("pitch", out var pitchProperty)
                    || pitchProperty.ValueKind != JsonValueKind.Number)
                {
                    continue;
                }

                var pitch = pitchProperty.GetDouble();
                pitchPattern.Add(pitch > 0.01d ? 1 : 0);
            }
        }

        return pitchPattern.Count == 0 ? null : pitchPattern;
    }

    private static string BuildCacheFileName(string text, int speaker)
    {
        using var sha256 = SHA256.Create();
        var raw = $"{speaker}:{text}";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"{hashHex}.wav";
    }

    private static string ResolveStoragePath(string storagePath)
    {
        if (Path.IsPathRooted(storagePath))
            return storagePath;

        return Path.Combine(Directory.GetCurrentDirectory(), storagePath);
    }

    private static string BuildPublicAudioUrl(string storagePath, string fileName)
    {
        var normalizedPath = storagePath.Replace('\\', '/').Trim('/');
        var publicFolder = normalizedPath.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase)
            ? normalizedPath["wwwroot/".Length..]
            : normalizedPath;

        return $"/{publicFolder}/{fileName}";
    }
}
