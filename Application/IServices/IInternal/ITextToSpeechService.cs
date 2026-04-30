namespace Application.IServices.IInternal;

public interface ITextToSpeechService
{
    /// <summary>
    /// Chuyển văn bản tiếng Nhật thành audio (WAV stream)
    /// </summary>
    Task<Stream> SynthesizeAsync(
        string text,
        string voiceName = "ja-JP-NanamiNeural",
        CancellationToken cancellationToken = default);
}
