using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Resources;

public class UploadImageFormRequest
{
    public IFormFile Image { get; set; } = null!;
}
