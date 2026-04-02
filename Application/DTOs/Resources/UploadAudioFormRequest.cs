using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Resources;

public class UploadAudioFormRequest
{
    public IFormFile Audio { get; set; } = null!;
}