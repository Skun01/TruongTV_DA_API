using Application.Common;
using Application.DTOs.Internal;
using Application.IServices.IInternal;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Helper;

/// <summary>
/// Helper chuẩn hóa flow Azure TTS → Cloudinary upload.
/// Nếu Azure chưa configured, throw ApplicationException với code chỉ định.
/// </summary>
public static class AzureTtsHelper
{
    /// <summary>
    /// Tổng hợp text bằng Azure TTS, upload lên Cloudinary, trả URL.
    /// Dùng cho Vocabulary, Sentence, Grammar — không dùng cho Exam (ExamService tự handle).
    /// </summary>
    /// <param name="ttsService">ITextToSpeechService (Azure impl)</param>
    /// <param name="fileUploadService">IFileUploadService</param>
    /// <param name="text">Văn bản tiếng Nhật cần tổng hợp</param>
    /// <param name="userId">User thực hiện action (dùng cho upload)</param>
    /// <param name="fileName">Tên file trên Cloudinary (nên đặt unique, có .mp3)</param>
    /// <param name="voiceName">Azure voice name, default ja-JP-NanamiNeural</param>
    /// <param name="errorCode">Code trả về khi lỗi (tùy ngữ cảnh: VOCABULARY_AUDIO_SYNTHESIS_FAILED, SENTENCE_AUDIO_SYNTHESIS_FAILED, GRAMMAR_AUDIO_SYNTHESIS_FAILED)</param>
    /// <param name="logger">ILogger (optional)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public static async Task<AudioSynthesisResult> SynthesizeAndUploadAsync(
        ITextToSpeechService ttsService,
        IFileUploadService fileUploadService,
        string text,
        string userId,
        string fileName,
        string errorCode,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new AudioSynthesisResult { AudioUrl = string.Empty };

        try
        {
            // Bước 1: Gọi Azure TTS → nhận audio stream (MP3)
            await using var audioStream = await ttsService.SynthesizeAsync(
                text,
                cancellationToken: cancellationToken);

            // Bước 2: Upload lên Cloudinary
            var uploadResult = await fileUploadService.UploadAsync(new FileUploadRequest
            {
                UserId = userId,
                FileName = fileName,
                ContentType = "audio/mpeg",
                Content = audioStream,
                FileType = FileType.Audio,
                UsageType = ResourceUsageType.Audio,
            }, cancellationToken);

            logger?.LogInformation(
                "Azure TTS + upload thành công. FileName: {FileName}, URL: {Url}",
                fileName, uploadResult.FileUrl);

            return new AudioSynthesisResult { AudioUrl = uploadResult.FileUrl };
        }
        catch (ApplicationException)
        {
            logger?.LogError("Azure TTS thất bại. Text: {Text}, ErrorCode: {ErrorCode}", text, errorCode);
            throw new AppException(errorCode, 500);
        }
    }
}