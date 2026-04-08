using Application.DTOs.Internal;
using Application.DTOs.Voicevox;

namespace Application.IServices.IInternal;

public interface IVoicevoxService
{
    Task<VoicevoxSynthesisResult?> SynthesizeAsync(string text, int? speakerId = null, CancellationToken cancellationToken = default);
    Task<List<VoicevoxSpeakerResponse>> GetSpeakersAsync(CancellationToken cancellationToken = default);
}
