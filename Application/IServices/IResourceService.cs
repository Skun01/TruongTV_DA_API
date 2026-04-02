using Application.DTOs.Resources;

namespace Application.IServices;

public interface IResourceService
{
    Task<UploadAudioResponse> UploadAudioAsync(string userId, UploadAudioRequest request, CancellationToken cancellationToken = default);
}
