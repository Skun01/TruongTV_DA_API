using Application.DTOs.Resources;
using Domain.Entities;

namespace Application.Mappings;

public static class ResourceMappings
{
    public static UploadAudioResponse ToUploadAudioResponse(this MediaAsset mediaAsset)
    {
        return new UploadAudioResponse
        {
            Id = mediaAsset.Id,
            FileUrl = mediaAsset.FileUrl,
            FileType = mediaAsset.FileType.ToString().ToLowerInvariant(),
            UsageType = mediaAsset.UsageType.ToString().ToLowerInvariant(),
            SizeInBytes = mediaAsset.SizeInBytes,
            CreatedAt = mediaAsset.CreatedAt,
        };
    }
}
