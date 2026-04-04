using Application.Common;
using Application.DTOs.Resources;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/uploads")]
public class ResourcesController : BaseController
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [Authorize]
    [HttpPost("audio")]
    public async Task<ApiResponse<UploadAudioResponse>> UploadAudio([FromForm] UploadAudioFormRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await using var stream = request.Audio.OpenReadStream();

        var response = await _resourceService.UploadAudioAsync(userId, new UploadAudioRequest
        {
            FileName = request.Audio.FileName,
            ContentType = request.Audio.ContentType,
            SizeInBytes = request.Audio.Length,
            Content = stream,
        }, cancellationToken);

        return ApiResponse<UploadAudioResponse>.SuccessResponse(response);
    }
}
