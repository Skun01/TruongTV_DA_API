using Application.Common;
using Application.DTOs.Voicevox;
using Application.IServices.IInternal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/voicevox")]
public class VoicevoxController : ControllerBase
{
    private readonly IVoicevoxService _voicevoxService;

    public VoicevoxController(IVoicevoxService voicevoxService)
    {
        _voicevoxService = voicevoxService;
    }

    [HttpGet("speakers")]
    public async Task<ApiResponse<List<VoicevoxSpeakerResponse>>> GetSpeakers(CancellationToken cancellationToken)
    {
        var speakers = await _voicevoxService.GetSpeakersAsync(cancellationToken);
        return ApiResponse<List<VoicevoxSpeakerResponse>>.SuccessResponse(speakers);
    }

    [HttpPost("preview")]
    public async Task<ApiResponse<VoicevoxPreviewResponse>> Preview(
        [FromBody] VoicevoxPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var preview = await _voicevoxService.PreviewAsync(request, cancellationToken);
        return ApiResponse<VoicevoxPreviewResponse>.SuccessResponse(preview);
    }
}
