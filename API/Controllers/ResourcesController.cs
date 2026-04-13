using Application.Common;
using Application.DTOs.Resources;
using Application.IServices;
using Domain.Constants;
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

    /// <summary>
    /// Tải lên tệp âm thanh và trả về thông tin tài nguyên.
    /// </summary>
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

    /// <summary>
    /// Tải lên tệp ảnh và trả về thông tin tài nguyên.
    /// </summary>
    [Authorize(Policy = AuthPolicyConstants.EditorOrAdmin)]
    [HttpPost("image")]
    public async Task<ApiResponse<UploadImageResponse>> UploadImage([FromForm] UploadImageFormRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await using var stream = request.Image.OpenReadStream();

        var response = await _resourceService.UploadImageAsync(userId, new UploadImageRequest
        {
            FileName = request.Image.FileName,
            ContentType = request.Image.ContentType,
            SizeInBytes = request.Image.Length,
            Content = stream,
        }, cancellationToken);

        return ApiResponse<UploadImageResponse>.SuccessResponse(response);
    }
}
