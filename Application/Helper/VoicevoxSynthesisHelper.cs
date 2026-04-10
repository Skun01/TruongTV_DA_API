using Application.Common;
using Application.DTOs.Internal;
using Application.IServices.IInternal;
using Domain.Constants;

namespace Application.Helper;

public static class VoicevoxSynthesisHelper
{
    public static Task<VoicevoxSynthesisResult> SynthesizeSentenceAsync(
        IVoicevoxService voicevoxService,
        string text,
        int? speakerId)
    {
        return SynthesizeAsync(voicevoxService, text, speakerId, MessageConstants.SentenceMessage.AUDIO_SYNTHESIS_FAILED);
    }

    public static Task<VoicevoxSynthesisResult> SynthesizeVocabularyAsync(
        IVoicevoxService voicevoxService,
        string text,
        int? speakerId)
    {
        return SynthesizeAsync(voicevoxService, text, speakerId, MessageConstants.VocabularyMessage.AUDIO_SYNTHESIS_FAILED);
    }

    private static async Task<VoicevoxSynthesisResult> SynthesizeAsync(
        IVoicevoxService voicevoxService,
        string text,
        int? speakerId,
        string errorCode)
    {
        try
        {
            return await voicevoxService.SynthesizeAsync(text, speakerId)
                ?? throw new AppException(errorCode, 500);
        }
        catch (AppException)
        {
            throw;
        }
        catch (ApplicationException ex) when (ex.Message == MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR)
        {
            throw new AppException(errorCode, 500);
        }
    }
}
